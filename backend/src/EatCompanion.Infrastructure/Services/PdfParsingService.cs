using EatCompanion.Application.Interfaces;
using EatCompanion.Domain.Entities;
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
}
