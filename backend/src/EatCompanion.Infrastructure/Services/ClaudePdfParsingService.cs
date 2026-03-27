using System.Text.Json;
using System.Text.Json.Serialization;
using Anthropic;
using Anthropic.Models.Messages;
using EatCompanion.Application.Interfaces;
using EatCompanion.Domain.Entities;
using EatCompanion.Domain.Enums;
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
        You are a nutritionist meal plan parser. You receive a PDF document from a Portuguese nutritionist containing a meal plan.
        Read the PDF visually and extract all the structured data from it.

        Your job:
        1. Identify all meal sections: Pequeno-almoço = Breakfast, Almoço = Lunch, Lanche da tarde = AfternoonSnack, Jantar = Dinner
        2. Each meal section may have multiple options separated by "OU" (meaning "OR"). Split them into separate options.
        3. If dinner has "Opção 1" and "Opção 2", treat as separate options within a single Dinner meal.
        4. For each option, extract EVERY individual ingredient as a separate item with quantity and unit.
        5. Translate ingredient names to English but keep the Portuguese name in namePt.
        6. Estimate realistic calories/protein/carbs/fat for each option based on the listed quantities.
        7. Generate a short, clean English name for each option (e.g., "Crepioca with Cheese and Yogurt").

        CRITICAL RULES FOR INGREDIENTS:
        - Split compound ingredients: "arroz/massa/batata" means the person will choose ONE of rice, pasta, or potato — list ALL as separate ingredients with the same quantity, and add "(or)" to clarify alternatives.
        - Split "+" separated items: "1 ovo + 2 colheres de sopa de tapioca + café" = 3 separate ingredients.
        - Every food item MUST be listed: eggs, bread, cheese, yogurt, fruit, coffee, olive oil, soup, salad, vegetables, rice, pasta, potato, meat, fish, chicken, oats, granola, crackers, cottage cheese, etc.
        - Include condiments and beverages: coffee, olive oil, mustard, salt, etc.
        - "Salada/legumes a gosto e/ou sopa" = 3 ingredients: salad (1 Units), vegetables (1 Units), soup (1 Units)
        - "150g de carne/peixe cozinhados ou 3 ovos" = 3 ingredients: meat (150 Grams), fish (150 Grams), eggs (3 Units) — they are alternatives
        - "1 porção de fruta" = fruit (1 Units)
        - Generic items like "café sem açúcar" = coffee (1 Units)

        Return ONLY valid JSON (no markdown, no code fences) in this exact format:
        {
          "meals": [
            {
              "mealType": "Breakfast",
              "options": [
                {
                  "name": "short English name",
                  "description": "original Portuguese text for this option, cleaned up",
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
        - Output exactly ONE entry per mealType. Do NOT duplicate Breakfast, Lunch, etc.
        - Order meals as: Breakfast, Lunch, AfternoonSnack, Dinner
        - Ignore recommendation sections ("Recomendações"), nutritionist names/watermarks, recipe appendices
        - Use these mealType values exactly: Breakfast, Lunch, AfternoonSnack, Dinner
        - Use these unit values exactly: Grams, Ml, Tablespoon, Teaspoon, Slice, Units
        - Use these category values exactly: Protein, Dairy, Grains, Produce, OilsAndCondiments, Other
        - Category guidance: meat/fish/chicken/eggs = Protein, cheese/yogurt/cottage = Dairy, rice/pasta/bread/oats/crackers = Grains, fruit/vegetables/salad = Produce, olive oil/coffee/mustard = OilsAndCondiments
        """;

    public ClaudePdfParsingService(IConfiguration configuration, ILogger<ClaudePdfParsingService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<MealPlan> ParseAsync(Stream pdfStream, string fileName, Guid userId)
    {
        // 1. Read PDF into bytes (send directly to Claude — no text extraction)
        using var memoryStream = new MemoryStream();
        await pdfStream.CopyToAsync(memoryStream);
        var pdfBytes = memoryStream.ToArray();
        var base64Pdf = Convert.ToBase64String(pdfBytes);

        _logger.LogInformation("Read PDF {FileName}: {Size} bytes", fileName, pdfBytes.Length);

        // 2. Send PDF to Claude API as a document
        var apiKey = _configuration["Anthropic:ApiKey"]
            ?? throw new InvalidOperationException("Anthropic API key is not configured. Set 'Anthropic:ApiKey' in configuration.");

        var parsed = await CallClaudeApi(apiKey, base64Pdf);
        if (parsed?.Meals?.Count > 0)
        {
            _logger.LogInformation("Claude parsed {MealCount} meals from PDF", parsed.Meals.Count);
            return BuildMealPlan(parsed, fileName, userId);
        }

        throw new InvalidOperationException("Failed to parse meal plan from PDF. Claude returned no meals.");
    }

    private async Task<ClaudeParsedPlan?> CallClaudeApi(string apiKey, string base64Pdf)
    {
        var client = new AnthropicClient { ApiKey = apiKey };

        var model = _configuration["Anthropic:Model"] ?? "claude-sonnet-4-20250514";

        // Send the PDF as a document block — Claude reads it visually
        var pdfSource = new Base64PdfSource(base64Pdf);
        var documentBlock = new DocumentBlockParam(pdfSource);
        var textBlock = new TextBlockParam { Text = "Parse this Portuguese nutritionist meal plan. Return ONLY valid JSON." };

        var message = await client.Messages.Create(new MessageCreateParams
        {
            Model = model,
            MaxTokens = 8192,
            System = SystemPrompt,
            Messages =
            [
                new()
                {
                    Role = Role.User,
                    Content = new List<ContentBlockParam> { documentBlock, textBlock }
                }
            ]
        });

        string? json = null;
        foreach (var block in message.Content)
        {
            if (block.TryPickText(out var textResult))
            {
                json = textResult.Text;
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
