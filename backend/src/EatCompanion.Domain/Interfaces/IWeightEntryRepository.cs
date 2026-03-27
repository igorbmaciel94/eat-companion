using EatCompanion.Domain.Entities;

namespace EatCompanion.Domain.Interfaces;

public interface IWeightEntryRepository
{
    Task<IReadOnlyList<WeightEntry>> GetByUserAndDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate);
    Task AddAsync(WeightEntry entry);
    Task<WeightEntry?> GetLatestAsync(Guid userId);
}
