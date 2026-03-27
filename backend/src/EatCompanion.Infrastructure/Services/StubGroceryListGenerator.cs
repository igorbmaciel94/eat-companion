using EatCompanion.Application.Interfaces;
using EatCompanion.Domain.Entities;

namespace EatCompanion.Infrastructure.Services;

/// <summary>
/// Stub implementation. Will be replaced by Track 5.
/// </summary>
public class StubGroceryListGenerator : IGroceryListGenerator
{
    public GroceryList Generate(MealPlan mealPlan, DateOnly startDate, DateOnly endDate, Guid userId)
    {
        return new GroceryList
        {
            Id = Guid.NewGuid(),
            MealPlanId = mealPlan.Id,
            UserId = userId,
            Name = $"Grocery List for {mealPlan.Name}",
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = DateTime.UtcNow,
            Items = new List<GroceryItem>()
        };
    }
}
