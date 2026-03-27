using EatCompanion.Domain.Entities;

namespace EatCompanion.Domain.Interfaces;

public interface IWeightEntryRepository
{
    Task<List<WeightEntry>> GetByUserAndDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate);
    Task<WeightEntry?> GetByIdAsync(Guid id);
    Task<WeightEntry?> GetByUserAndDateAsync(Guid userId, DateOnly date);
    Task AddAsync(WeightEntry weightEntry);
    void Update(WeightEntry weightEntry);
}
