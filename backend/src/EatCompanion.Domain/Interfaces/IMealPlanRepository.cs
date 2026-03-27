using EatCompanion.Domain.Entities;

namespace EatCompanion.Domain.Interfaces;

public interface IMealPlanRepository
{
    Task<MealPlan?> GetByIdAsync(Guid id);
    Task<MealPlan?> GetByIdWithDetailsAsync(Guid id);
    Task<List<MealPlan>> GetByUserIdAsync(Guid userId);
    Task<MealPlanDay?> GetDayAsync(Guid mealPlanId, DateOnly date);
    Task<MealOption?> GetMealOptionAsync(Guid optionId);
    Task<List<MealOption>> GetSiblingOptionsAsync(Guid mealId);
    Task AddAsync(MealPlan mealPlan);
    void Update(MealPlan mealPlan);
    void UpdateOption(MealOption option);
}
