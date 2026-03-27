using EatCompanion.Domain.Entities;

namespace EatCompanion.Domain.Interfaces;

public interface IMealLogRepository
{
    Task<List<MealLog>> GetByUserAndDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate);
    Task<MealLog?> GetByUserDateAndOptionAsync(Guid userId, DateOnly date, Guid? mealOptionId);
    Task AddAsync(MealLog mealLog);
    void Update(MealLog mealLog);
    void Delete(MealLog mealLog);
}
