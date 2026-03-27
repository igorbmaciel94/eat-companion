using EatCompanion.Domain.Entities;
using EatCompanion.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EatCompanion.Infrastructure.Persistence.Repositories;

public class MealPlanRepository : IMealPlanRepository
{
    private readonly AppDbContext _context;

    public MealPlanRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MealPlan?> GetByIdAsync(Guid id)
    {
        return await _context.MealPlans
            .Include(mp => mp.Days)
                .ThenInclude(d => d.Meals)
                    .ThenInclude(m => m.Options)
                        .ThenInclude(o => o.Ingredients)
            .FirstOrDefaultAsync(mp => mp.Id == id);
    }

    public async Task<IReadOnlyList<MealPlan>> GetByUserIdAsync(Guid userId)
    {
        return await _context.MealPlans
            .Where(mp => mp.UserId == userId)
            .OrderByDescending(mp => mp.ImportedAt)
            .ToListAsync();
    }

    public async Task<MealPlanDay?> GetDayAsync(Guid mealPlanId, DateOnly date)
    {
        return await _context.MealPlanDays
            .Include(d => d.Meals)
                .ThenInclude(m => m.Options)
                    .ThenInclude(o => o.Ingredients)
            .FirstOrDefaultAsync(d => d.MealPlanId == mealPlanId && d.Date == date);
    }

    public async Task AddAsync(MealPlan mealPlan)
    {
        await _context.MealPlans.AddAsync(mealPlan);
    }

    public void Update(MealPlan mealPlan)
    {
        _context.MealPlans.Update(mealPlan);
    }
}
