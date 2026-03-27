using EatCompanion.Domain.Entities;
using EatCompanion.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EatCompanion.Infrastructure.Persistence.Repositories;

public class WeightEntryRepository : IWeightEntryRepository
{
    private readonly AppDbContext _context;

    public WeightEntryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<WeightEntry>> GetByUserAndDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate)
    {
        return await _context.WeightEntries
            .Where(we => we.UserId == userId && we.Date >= startDate && we.Date <= endDate)
            .OrderBy(we => we.Date)
            .ToListAsync();
    }

    public async Task AddAsync(WeightEntry entry)
    {
        await _context.WeightEntries.AddAsync(entry);
    }

    public async Task<WeightEntry?> GetLatestAsync(Guid userId)
    {
        return await _context.WeightEntries
            .Where(we => we.UserId == userId)
            .OrderByDescending(we => we.Date)
            .FirstOrDefaultAsync();
    }
}
