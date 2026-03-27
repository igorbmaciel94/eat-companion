using EatCompanion.Application.Interfaces;
using EatCompanion.Domain.Entities;
using EatCompanion.Domain.Enums;
using EatCompanion.Infrastructure.PdfParsing;

namespace EatCompanion.Infrastructure.Services;

public class PdfParsingService : IPdfParsingService
{
    public Task<MealPlan> ParseAsync(Stream pdfStream, string fileName, Guid userId)
    {
        // 1. Extract text pages
        var extractor = new PdfTextExtractor();
        var pages = extractor.ExtractPages(pdfStream);

        // 2. Parse sections and options
        var parser = new MealPlanParser();
        var parsedPlan = parser.Parse(pages);

        // 3. Normalize ingredients and build domain entities
        var normalizer = new IngredientNormalizer();

        var mealPlan = new MealPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = Path.GetFileNameWithoutExtension(fileName),
            SourceFileName = fileName,
            ImportedAt = DateTime.UtcNow,
            IsActive = true
        };

        // Create a template day (day 0) since the PDF is not date-specific
        var templateDay = BuildDay(parsedPlan, normalizer, 0, mealPlan.Id);
        mealPlan.Days.Add(templateDay);

        // Clone for days 1-6 (full week)
        for (int i = 1; i <= 6; i++)
        {
            var clonedDay = CloneDay(templateDay, i, mealPlan.Id);
            mealPlan.Days.Add(clonedDay);
        }

        return Task.FromResult(mealPlan);
    }

    private static MealPlanDay BuildDay(ParsedMealPlan parsedPlan, IngredientNormalizer normalizer, int dayOfWeek, Guid mealPlanId)
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
                    Name = GenerateOptionName(parsedOption),
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

                EstimateNutrition(option);
                meal.Options.Add(option);
            }

            day.Meals.Add(meal);
        }

        return day;
    }

    private static string GenerateOptionName(ParsedMealOption parsedOption)
    {
        // Build a short readable name from the main ingredients
        if (parsedOption.Ingredients.Count == 0)
        {
            // Fallback: take first ~50 chars of description
            var desc = parsedOption.Description;
            if (desc.Length > 50)
                desc = desc[..50] + "...";
            return desc;
        }

        // Take the top 2-3 main ingredients (skip generic items like coffee, oil)
        var genericItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "café", "azeite", "salada", "legumes", "sopa"
        };

        var mainIngredients = parsedOption.Ingredients
            .Where(i => !genericItems.Contains(i.NamePt))
            .Take(3)
            .Select(i => CapitalizeFirst(i.NamePt))
            .ToList();

        if (mainIngredients.Count == 0)
        {
            mainIngredients = parsedOption.Ingredients
                .Take(2)
                .Select(i => CapitalizeFirst(i.NamePt))
                .ToList();
        }

        return mainIngredients.Count switch
        {
            0 => parsedOption.Description.Length > 50
                ? parsedOption.Description[..50] + "..."
                : parsedOption.Description,
            1 => mainIngredients[0],
            2 => $"{mainIngredients[0]} com {mainIngredients[1]}",
            _ => $"{mainIngredients[0]} com {mainIngredients[1]} e {mainIngredients[2]}"
        };
    }

    private static string CapitalizeFirst(string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;
        // Remove quantities at the start like "arroz/massa/batata"
        s = s.Trim();
        return char.ToUpper(s[0]) + s[1..];
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

    private static void EstimateNutrition(MealOption option)
    {
        int totalCal = 0;
        decimal totalProtein = 0, totalCarbs = 0, totalFat = 0;

        foreach (var ing in option.Ingredients)
        {
            var (cal, prot, carbs, fat) = LookupNutrition(ing.NamePt ?? ing.Name, ing.Amount, ing.Unit);
            totalCal += cal;
            totalProtein += prot;
            totalCarbs += carbs;
            totalFat += fat;
        }

        option.Calories = totalCal > 0 ? totalCal : null;
        option.ProteinGrams = totalProtein > 0 ? totalProtein : null;
        option.CarbsGrams = totalCarbs > 0 ? totalCarbs : null;
        option.FatGrams = totalFat > 0 ? totalFat : null;
    }

    // Nutrition per standard serving: (kcal, protein_g, carbs_g, fat_g)
    // Amounts are scaled based on ingredient unit and quantity
    private static readonly List<(string Pattern, decimal BaseAmount, UnitOfMeasure BaseUnit, int Kcal, decimal Protein, decimal Carbs, decimal Fat)> NutritionTable = new()
    {
        // Grains (per 100g cooked)
        ("arroz", 100, UnitOfMeasure.Grams, 130, 3, 28, 0.5m),
        ("massa", 100, UnitOfMeasure.Grams, 130, 5, 25, 1),
        ("batata", 100, UnitOfMeasure.Grams, 80, 2, 18, 0),
        ("pão", 60, UnitOfMeasure.Grams, 160, 5, 30, 1),
        ("tapioca", 1, UnitOfMeasure.Tablespoon, 30, 0, 7, 0),
        ("flocos de aveia", 20, UnitOfMeasure.Grams, 75, 2.5m, 13, 1.5m),
        ("granola", 20, UnitOfMeasure.Grams, 80, 2, 14, 2),
        ("cereais", 20, UnitOfMeasure.Grams, 80, 2, 14, 2),
        ("bolachas", 1, UnitOfMeasure.Units, 35, 1, 7, 0.3m),
        ("tortitas", 1, UnitOfMeasure.Units, 30, 0.5m, 6, 0.3m),
        ("tortilha", 1, UnitOfMeasure.Units, 120, 3, 20, 3),

        // Protein (per 100g)
        ("carne", 100, UnitOfMeasure.Grams, 150, 25, 0, 5),
        ("peixe", 100, UnitOfMeasure.Grams, 120, 22, 0, 3),
        ("frango", 100, UnitOfMeasure.Grams, 165, 31, 0, 3.6m),
        ("ovo", 1, UnitOfMeasure.Units, 78, 6, 0.6m, 5),

        // Dairy
        ("queijo flamengo", 1, UnitOfMeasure.Slice, 50, 4, 0, 3.5m),
        ("queijo cottage", 30, UnitOfMeasure.Grams, 25, 4, 1, 0.5m),
        ("queijo creme", 1, UnitOfMeasure.Units, 30, 1.5m, 1, 2),
        ("queijinho", 1, UnitOfMeasure.Units, 35, 2, 1, 2.5m),
        ("iogurte", 1, UnitOfMeasure.Units, 80, 12, 8, 0.5m),
        ("pudim proteico", 1, UnitOfMeasure.Units, 120, 20, 10, 1),

        // Produce
        ("fruta", 1, UnitOfMeasure.Units, 60, 0.5m, 15, 0),
        ("salada", 1, UnitOfMeasure.Units, 25, 1, 5, 0),
        ("legumes", 1, UnitOfMeasure.Units, 25, 1, 5, 0),
        ("sopa", 1, UnitOfMeasure.Units, 60, 2, 8, 2),

        // Oils & Other
        ("azeite", 1, UnitOfMeasure.Units, 90, 0, 0, 10),
        ("café", 1, UnitOfMeasure.Units, 2, 0, 0, 0),
    };

    private static (int Kcal, decimal Protein, decimal Carbs, decimal Fat) LookupNutrition(
        string namePt, decimal amount, UnitOfMeasure unit)
    {
        var lower = namePt.ToLowerInvariant();

        foreach (var entry in NutritionTable)
        {
            if (!lower.Contains(entry.Pattern, StringComparison.OrdinalIgnoreCase))
                continue;

            // Scale based on amount and unit
            decimal scale;
            if (entry.BaseUnit == unit)
            {
                scale = amount / entry.BaseAmount;
            }
            else if (unit == UnitOfMeasure.Grams && entry.BaseUnit == UnitOfMeasure.Grams)
            {
                scale = amount / entry.BaseAmount;
            }
            else
            {
                // Different units — assume 1:1 serving
                scale = amount;
            }

            return (
                (int)(entry.Kcal * scale),
                entry.Protein * scale,
                entry.Carbs * scale,
                entry.Fat * scale
            );
        }

        return (0, 0, 0, 0);
    }
}
