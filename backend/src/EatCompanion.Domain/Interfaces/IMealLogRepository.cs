using EatCompanion.Domain.Entities;

namespace EatCompanion.Domain.Interfaces;

public interface IMealLogRepository
{
    Task<List<MealLog>> GetByUserAndDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate);
    Task AddAsync(MealLog mealLog);
}
