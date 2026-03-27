using EatCompanion.Domain.Entities;
using EatCompanion.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EatCompanion.Infrastructure.Persistence.Repositories;

public class MealLogRepository : IMealLogRepository
{
    private readonly AppDbContext _context;

    public MealLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MealLog>> GetByUserAndDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate)
    {
        return await _context.MealLogs
            .Include(ml => ml.MealOption)
            .Where(ml => ml.UserId == userId && ml.Date >= startDate && ml.Date <= endDate)
            .OrderBy(ml => ml.Date)
            .ThenBy(ml => ml.MealType)
            .ToListAsync();
    }

    public async Task AddAsync(MealLog mealLog)
    {
        await _context.MealLogs.AddAsync(mealLog);
    }
}
