using EatCompanion.Domain.Entities;

namespace EatCompanion.Application.Interfaces;

public interface IGroceryListGenerator
{
    GroceryList Generate(MealPlan mealPlan, DateOnly startDate, DateOnly endDate, Guid userId);
}
