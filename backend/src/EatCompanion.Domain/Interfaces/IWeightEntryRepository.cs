using EatCompanion.Domain.Entities;

namespace EatCompanion.Domain.Interfaces;

public interface IWeightEntryRepository
{
    Task<List<WeightEntry>> GetByUserAndDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate);
    Task AddAsync(WeightEntry weightEntry);
}
