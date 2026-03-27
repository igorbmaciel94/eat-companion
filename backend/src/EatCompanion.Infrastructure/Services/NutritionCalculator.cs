using EatCompanion.Application.DTOs;
using EatCompanion.Application.Interfaces;
using EatCompanion.Domain.Entities;
using EatCompanion.Domain.Enums;

namespace EatCompanion.Infrastructure.Services;

public class NutritionCalculator : INutritionCalculator
{
    public DailySummaryDto CalculateDailySummary(MealPlanDay day, IEnumerable<MealLog> logs, int calorieTarget)
    {
        var logList = logs.ToList();

        // Calculate consumed totals from selected meal options
        int consumedCalories = 0;
        decimal consumedProtein = 0, consumedCarbs = 0, consumedFat = 0;

        foreach (var meal in day.Meals)
        {
            var selectedOption = meal.Options.FirstOrDefault(o => o.IsSelected)
                                 ?? meal.Options.FirstOrDefault();

            if (selectedOption is null) continue;

            consumedCalories += selectedOption.Calories ?? 0;
            consumedProtein += selectedOption.ProteinGrams ?? 0;
            consumedCarbs += selectedOption.CarbsGrams ?? 0;
            consumedFat += selectedOption.FatGrams ?? 0;
        }

        var meals = day.Meals.Select(m => new MealDto(
            m.Id,
            m.MealType,
            m.SortOrder,
            m.Options.Select(o => new MealOptionDto(
                o.Id,
                o.Name,
                o.Description ?? o.Name,
                o.IsSelected,
                o.SortOrder,
                o.Calories,
                o.ProteinGrams,
                o.CarbsGrams,
                o.FatGrams,
                o.Ingredients.Select(i => new IngredientDto(
                    i.Id,
                    i.Name,
                    i.NamePt ?? i.Name,
                    i.Amount,
                    i.Unit,
                    i.Category
                )).ToList()
            )).ToList()
        )).ToList();

        return new DailySummaryDto(
            day.Date,
            calorieTarget,
            consumedCalories,
            consumedProtein,
            consumedCarbs,
            consumedFat,
            0,
            meals
        );
    }
}
