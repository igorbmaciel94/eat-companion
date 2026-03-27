using EatCompanion.Domain.Entities;

namespace EatCompanion.Domain.Interfaces;

public interface IMealPlanRepository
{
    Task<MealPlan?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<MealPlan>> GetByUserIdAsync(Guid userId);
    Task<MealPlanDay?> GetDayAsync(Guid mealPlanId, DateOnly date);
    Task AddAsync(MealPlan mealPlan);
    void Update(MealPlan mealPlan);
}
