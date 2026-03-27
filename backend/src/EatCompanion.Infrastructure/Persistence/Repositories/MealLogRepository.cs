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

    public async Task<List<MealLog>> GetByUserAndDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate)
    {
        return await _context.MealLogs
            .Include(ml => ml.MealOption)
            .Where(ml => ml.UserId == userId && ml.Date >= startDate && ml.Date <= endDate)
            .OrderBy(ml => ml.Date)
            .ThenBy(ml => ml.MealType)
            .ToListAsync();
    }

    public async Task<MealLog?> GetByUserDateAndOptionAsync(Guid userId, DateOnly date, Guid? mealOptionId)
    {
        return await _context.MealLogs
            .FirstOrDefaultAsync(ml => ml.UserId == userId && ml.Date == date && ml.MealOptionId == mealOptionId);
    }

    public async Task AddAsync(MealLog mealLog)
    {
        await _context.MealLogs.AddAsync(mealLog);
    }

    public void Update(MealLog mealLog)
    {
        _context.MealLogs.Update(mealLog);
    }

    public void Delete(MealLog mealLog)
    {
        _context.MealLogs.Remove(mealLog);
    }
}
