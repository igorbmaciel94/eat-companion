using EatCompanion.Application.Interfaces;
using EatCompanion.Domain.Entities;
using EatCompanion.Domain.Enums;

namespace EatCompanion.Infrastructure.Services;

public class GroceryListGenerator : IGroceryListGenerator
{
    public GroceryList Generate(MealPlan mealPlan, DateOnly startDate, DateOnly endDate, Guid userId)
    {
        var groceryList = new GroceryList
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MealPlanId = mealPlan.Id,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = DateTime.UtcNow
        };

        // Calculate how many days in the range
        var totalDays = endDate.DayNumber - startDate.DayNumber + 1;
        if (totalDays <= 0) totalDays = 1;

        // Collect all selected ingredients across the date range
        // Key: (display name, category, unit) → (total amount, english name, portuguese name)
        var ingredientAggregator = new Dictionary<(string Key, IngredientCategory Category, UnitOfMeasure Unit), (decimal Amount, string Name, string? NamePt)>();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dayOfWeek = (int)date.DayOfWeek; // Sunday=0

            // Find the matching day in the meal plan
            var mealPlanDay = mealPlan.Days.FirstOrDefault(d => d.DayOfWeek == dayOfWeek)
                              ?? mealPlan.Days.FirstOrDefault(); // fallback to template

            if (mealPlanDay is null) continue;

            foreach (var meal in mealPlanDay.Meals)
            {
                // Take the selected option (or first if none selected)
                var selectedOption = meal.Options.FirstOrDefault(o => o.IsSelected)
                                     ?? meal.Options.FirstOrDefault();

                if (selectedOption is null) continue;

                foreach (var ingredient in selectedOption.Ingredients)
                {
                    // Use Portuguese name as key for better grouping
                    var displayKey = (ingredient.NamePt ?? ingredient.Name).ToLowerInvariant().Trim();
                    var key = (displayKey, ingredient.Category, ingredient.Unit);
                    if (ingredientAggregator.TryGetValue(key, out var existing))
                    {
                        ingredientAggregator[key] = (existing.Amount + ingredient.Amount, existing.Name, existing.NamePt ?? ingredient.NamePt);
                    }
                    else
                    {
                        ingredientAggregator[key] = (ingredient.Amount, ingredient.Name, ingredient.NamePt);
                    }
                }
            }
        }

        // Create grocery items from aggregated ingredients
        foreach (var ((_, category, unit), (totalAmount, name, namePt)) in ingredientAggregator)
        {
            groceryList.Items.Add(new GroceryItem
            {
                Id = Guid.NewGuid(),
                GroceryListId = groceryList.Id,
                Name = name,
                NamePt = namePt,
                Category = category,
                TotalAmount = totalAmount,
                Unit = unit,
                IsChecked = false
            });
        }

        // Sort by category then name
        groceryList.Items = groceryList.Items
            .OrderBy(i => i.Category)
            .ThenBy(i => i.Name)
            .ToList();

        return groceryList;
    }
}
