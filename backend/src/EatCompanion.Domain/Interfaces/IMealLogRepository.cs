using EatCompanion.Domain.Entities;

namespace EatCompanion.Domain.Interfaces;

public interface IMealLogRepository
{
    Task<IReadOnlyList<MealLog>> GetByUserAndDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate);
    Task AddAsync(MealLog mealLog);
}
