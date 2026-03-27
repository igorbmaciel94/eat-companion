using System.Text.Json;
using System.Text.Json.Serialization;
using Anthropic;
using Anthropic.Models.Messages;
using EatCompanion.Application.Interfaces;
using EatCompanion.Domain.Entities;
using EatCompanion.Domain.Enums;
using EatCompanion.Infrastructure.PdfParsing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EatCompanion.Infrastructure.Services;

public class ClaudePdfParsingService : IPdfParsingService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ClaudePdfParsingService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private const string SystemPrompt = """
        You are a nutritionist meal plan parser. You receive raw text extracted from a Portuguese nutritionist's PDF meal plan.

        Your job:
        1. Identify all meal sections (Pequeno-almoço = Breakfast, Almoço = Lunch, Lanche da tarde = AfternoonSnack, Jantar = Dinner)
        2. For each meal, identify all OR-options (separated by "OU" in Portuguese)
        3. For each option, extract the ingredients with quantities and units
        4. Translate ingredient names to English
        5. Estimate calories, protein, carbs, and fat for each option based on the ingredients and quantities
        6. Generate a short, clean English name for each option (e.g., "Eggs with Toast and Fruit")

        Return ONLY valid JSON (no markdown, no code fences) in this exact format:
        {
          "meals": [
            {
              "mealType": "Breakfast",
              "options": [
                {
                  "name": "short English name for this option",
                  "description": "original Portuguese text for this option",
                  "calories": 450,
                  "proteinGrams": 25.0,
                  "carbsGrams": 40.0,
                  "fatGrams": 15.0,
                  "ingredients": [
                    {
                      "name": "English name",
                      "namePt": "Portuguese name",
                      "amount": 150.0,
                      "unit": "Grams",
                      "category": "Protein"
                    }
                  ]
                }
              ]
            }
          ]
        }

        Rules:
        - Ignore recommendation sections, nutritionist names/watermarks, recipe sections at the end
        - If the text mentions "OU" (meaning "OR"), split into separate options
        - If dinner has "Opção 1" and "Opção 2", treat as separate options
        - Estimate calories realistically based on the ingredients and quantities listed
        - Use these mealType values exactly: Breakfast, Lunch, AfternoonSnack, Dinner
        - Use these unit values exactly: Grams, Ml, Tablespoon, Teaspoon, Slice, Units
        - Use these category values exactly: Protein, Dairy, Grains, Produce, OilsAndCondiments, Other
        - Keep the original Portuguese text in the "description" field for each option
        - Keep the Portuguese ingredient name in "namePt"
        """;

    public ClaudePdfParsingService(IConfiguration configuration, ILogger<ClaudePdfParsingService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<MealPlan> ParseAsync(Stream pdfStream, string fileName, Guid userId)
    {
        // 1. Extract text from PDF
        var extractor = new PdfTextExtractor();
        var pages = extractor.ExtractPages(pdfStream);
        var fullText = string.Join("\n---PAGE BREAK---\n", pages);

        _logger.LogInformation("Extracted {PageCount} pages from PDF, total {Length} chars", pages.Count, fullText.Length);

        // 2. Try Claude API
        var apiKey = _configuration["Anthropic:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
        {
            try
            {
                var parsed = await CallClaudeApi(apiKey, fullText);
                if (parsed?.Meals?.Count > 0)
                {
                    _logger.LogInformation("Claude parsed {MealCount} meals from PDF", parsed.Meals.Count);
                    return BuildMealPlan(parsed, fileName, userId);
                }
                _logger.LogWarning("Claude returned empty meals, falling back to regex parser");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Claude API failed, falling back to regex parser");
            }
        }
        else
        {
            _logger.LogWarning("No Anthropic API key configured, using regex parser fallback");
        }

        // 3. Fallback to regex-based parser
        return FallbackParse(pages, fileName, userId);
    }

    private async Task<ClaudeParsedPlan?> CallClaudeApi(string apiKey, string fullText)
    {
        var client = new AnthropicClient { ApiKey = apiKey };

        var model = _configuration["Anthropic:Model"] ?? "claude-sonnet-4-20250514";

        var message = await client.Messages.Create(new MessageCreateParams
        {
            Model = model,
            MaxTokens = 4096,
            System = SystemPrompt,
            Messages =
            [
                new()
                {
                    Role = Role.User,
                    Content = $"Parse this Portuguese nutritionist meal plan:\n\n{fullText}"
                }
            ]
        });

        string? json = null;
        foreach (var block in message.Content)
        {
            if (block.TryPickText(out var textBlock))
            {
                json = textBlock.Text;
                break;
            }
        }

        if (string.IsNullOrEmpty(json))
            return null;

        // Strip markdown code fences if present
        json = json.Trim();
        if (json.StartsWith("```"))
        {
            var firstNewline = json.IndexOf('\n');
            if (firstNewline > 0) json = json[(firstNewline + 1)..];
            if (json.EndsWith("```")) json = json[..^3];
            json = json.Trim();
        }

        _logger.LogDebug("Claude response JSON: {Json}", json);

        return JsonSerializer.Deserialize<ClaudeParsedPlan>(json, JsonOptions);
    }

    private static MealPlan BuildMealPlan(ClaudeParsedPlan parsed, string fileName, Guid userId)
    {
        var mealPlan = new MealPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = Path.GetFileNameWithoutExtension(fileName),
            SourceFileName = fileName,
            ImportedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Build template day (day 0)
        var templateDay = BuildDay(parsed, 0, mealPlan.Id);
        mealPlan.Days.Add(templateDay);

        // Clone for days 1-6
        for (int i = 1; i <= 6; i++)
        {
            mealPlan.Days.Add(CloneDay(templateDay, i, mealPlan.Id));
        }

        return mealPlan;
    }

    private static MealPlanDay BuildDay(ClaudeParsedPlan parsed, int dayOfWeek, Guid mealPlanId)
    {
        var day = new MealPlanDay
        {
            Id = Guid.NewGuid(),
            MealPlanId = mealPlanId,
            DayOfWeek = dayOfWeek
        };

        foreach (var parsedMeal in parsed.Meals)
        {
            var mealType = ParseMealType(parsedMeal.MealType);

            var meal = new Meal
            {
                Id = Guid.NewGuid(),
                MealPlanDayId = day.Id,
                MealType = mealType,
                SortOrder = (int)mealType
            };

            var sortOrder = 0;
            foreach (var parsedOption in parsedMeal.Options)
            {
                var option = new MealOption
                {
                    Id = Guid.NewGuid(),
                    MealId = meal.Id,
                    Name = parsedOption.Name,
                    Description = parsedOption.Description,
                    IsSelected = sortOrder == 0,
                    SortOrder = sortOrder++,
                    Calories = parsedOption.Calories > 0 ? parsedOption.Calories : null,
                    ProteinGrams = parsedOption.ProteinGrams > 0 ? parsedOption.ProteinGrams : null,
                    CarbsGrams = parsedOption.CarbsGrams > 0 ? parsedOption.CarbsGrams : null,
                    FatGrams = parsedOption.FatGrams > 0 ? parsedOption.FatGrams : null
                };

                foreach (var parsedIngredient in parsedOption.Ingredients)
                {
                    option.Ingredients.Add(new Ingredient
                    {
                        Id = Guid.NewGuid(),
                        MealOptionId = option.Id,
                        Name = parsedIngredient.Name,
                        NamePt = parsedIngredient.NamePt,
                        Amount = parsedIngredient.Amount,
                        Unit = ParseUnit(parsedIngredient.Unit),
                        Category = ParseCategory(parsedIngredient.Category)
                    });
                }

                meal.Options.Add(option);
            }

            day.Meals.Add(meal);
        }

        return day;
    }

    private static MealType ParseMealType(string value)
    {
        return value?.Trim() switch
        {
            "Breakfast" => MealType.Breakfast,
            "Lunch" => MealType.Lunch,
            "AfternoonSnack" => MealType.AfternoonSnack,
            "Dinner" => MealType.Dinner,
            "MorningSnack" => MealType.MorningSnack,
            "EveningSnack" => MealType.EveningSnack,
            "Snack" => MealType.Snack,
            _ => Enum.TryParse<MealType>(value, true, out var parsed) ? parsed : MealType.Snack
        };
    }

    private static UnitOfMeasure ParseUnit(string value)
    {
        return value?.Trim() switch
        {
            "Grams" => UnitOfMeasure.Grams,
            "Ml" => UnitOfMeasure.Ml,
            "Tablespoon" => UnitOfMeasure.Tablespoon,
            "Teaspoon" => UnitOfMeasure.Teaspoon,
            "Slice" => UnitOfMeasure.Slice,
            "Units" => UnitOfMeasure.Units,
            _ => Enum.TryParse<UnitOfMeasure>(value, true, out var parsed) ? parsed : UnitOfMeasure.Units
        };
    }

    private static IngredientCategory ParseCategory(string value)
    {
        return value?.Trim() switch
        {
            "Protein" => IngredientCategory.Protein,
            "Dairy" => IngredientCategory.Dairy,
            "Grains" => IngredientCategory.Grains,
            "Produce" => IngredientCategory.Produce,
            "OilsAndCondiments" => IngredientCategory.OilsAndCondiments,
            "Other" => IngredientCategory.Other,
            _ => Enum.TryParse<IngredientCategory>(value, true, out var parsed) ? parsed : IngredientCategory.Other
        };
    }

    private static MealPlanDay CloneDay(MealPlanDay template, int dayOfWeek, Guid mealPlanId)
    {
        var day = new MealPlanDay
        {
            Id = Guid.NewGuid(),
            MealPlanId = mealPlanId,
            DayOfWeek = dayOfWeek
        };

        foreach (var templateMeal in template.Meals)
        {
            var meal = new Meal
            {
                Id = Guid.NewGuid(),
                MealPlanDayId = day.Id,
                MealType = templateMeal.MealType,
                SortOrder = templateMeal.SortOrder
            };

            foreach (var templateOption in templateMeal.Options)
            {
                var option = new MealOption
                {
                    Id = Guid.NewGuid(),
                    MealId = meal.Id,
                    Name = templateOption.Name,
                    Description = templateOption.Description,
                    IsSelected = templateOption.IsSelected,
                    SortOrder = templateOption.SortOrder,
                    Calories = templateOption.Calories,
                    ProteinGrams = templateOption.ProteinGrams,
                    CarbsGrams = templateOption.CarbsGrams,
                    FatGrams = templateOption.FatGrams
                };

                foreach (var templateIngredient in templateOption.Ingredients)
                {
                    option.Ingredients.Add(new Ingredient
                    {
                        Id = Guid.NewGuid(),
                        MealOptionId = option.Id,
                        Name = templateIngredient.Name,
                        NamePt = templateIngredient.NamePt,
                        Amount = templateIngredient.Amount,
                        Unit = templateIngredient.Unit,
                        Category = templateIngredient.Category
                    });
                }

                meal.Options.Add(option);
            }

            day.Meals.Add(meal);
        }

        return day;
    }

    private static MealPlan FallbackParse(IReadOnlyList<string> pages, string fileName, Guid userId)
    {
        // Use the old regex-based parser as fallback
        var parser = new MealPlanParser();
        var parsedPlan = parser.Parse(pages);
        var normalizer = new IngredientNormalizer();

        var fallbackService = new PdfParsingService();
        // Reconstruct via the old service — we call its internal logic
        return BuildFallbackMealPlan(parsedPlan, normalizer, fileName, userId);
    }

    private static MealPlan BuildFallbackMealPlan(ParsedMealPlan parsedPlan, IngredientNormalizer normalizer, string fileName, Guid userId)
    {
        var mealPlan = new MealPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = Path.GetFileNameWithoutExtension(fileName),
            SourceFileName = fileName,
            ImportedAt = DateTime.UtcNow,
            IsActive = true
        };

        var templateDay = BuildFallbackDay(parsedPlan, normalizer, 0, mealPlan.Id);
        mealPlan.Days.Add(templateDay);

        for (int i = 1; i <= 6; i++)
        {
            mealPlan.Days.Add(CloneDay(templateDay, i, mealPlan.Id));
        }

        return mealPlan;
    }

    private static MealPlanDay BuildFallbackDay(ParsedMealPlan parsedPlan, IngredientNormalizer normalizer, int dayOfWeek, Guid mealPlanId)
    {
        var day = new MealPlanDay
        {
            Id = Guid.NewGuid(),
            MealPlanId = mealPlanId,
            DayOfWeek = dayOfWeek
        };

        foreach (var parsedMeal in parsedPlan.Meals)
        {
            var meal = new Meal
            {
                Id = Guid.NewGuid(),
                MealPlanDayId = day.Id,
                MealType = parsedMeal.Type,
                SortOrder = (int)parsedMeal.Type
            };

            var sortOrder = 0;
            foreach (var parsedOption in parsedMeal.Options)
            {
                var option = new MealOption
                {
                    Id = Guid.NewGuid(),
                    MealId = meal.Id,
                    Name = parsedOption.Description.Length > 50
                        ? parsedOption.Description[..50] + "..."
                        : parsedOption.Description,
                    Description = parsedOption.Description,
                    IsSelected = sortOrder == 0,
                    SortOrder = sortOrder++
                };

                foreach (var parsedIngredient in parsedOption.Ingredients)
                {
                    var (englishName, category) = normalizer.Normalize(parsedIngredient.NamePt);
                    option.Ingredients.Add(new Ingredient
                    {
                        Id = Guid.NewGuid(),
                        MealOptionId = option.Id,
                        Name = englishName,
                        NamePt = parsedIngredient.NamePt,
                        Amount = parsedIngredient.Amount,
                        Unit = parsedIngredient.Unit,
                        Category = category
                    });
                }

                meal.Options.Add(option);
            }

            day.Meals.Add(meal);
        }

        return day;
    }

    // JSON deserialization types
    private record ClaudeParsedPlan(List<ClaudeParsedMeal> Meals);
    private record ClaudeParsedMeal(string MealType, List<ClaudeParsedOption> Options);
    private record ClaudeParsedOption(
        string Name,
        string Description,
        int Calories,
        decimal ProteinGrams,
        decimal CarbsGrams,
        decimal FatGrams,
        List<ClaudeParsedIngredient> Ingredients);
    private record ClaudeParsedIngredient(
        string Name,
        string NamePt,
        decimal Amount,
        string Unit,
        string Category);
}
