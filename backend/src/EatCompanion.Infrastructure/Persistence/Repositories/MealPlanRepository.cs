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

    public async Task<MealPlan?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.MealPlans
            .Include(mp => mp.Days)
                .ThenInclude(d => d.Meals)
                    .ThenInclude(m => m.Options)
                        .ThenInclude(o => o.Ingredients)
            .FirstOrDefaultAsync(mp => mp.Id == id);
    }

    public async Task<List<MealPlan>> GetByUserIdAsync(Guid userId)
    {
        return await _context.MealPlans
            .Where(mp => mp.UserId == userId && mp.IsActive)
            .OrderByDescending(mp => mp.ImportedAt)
            .ToListAsync();
    }

    public async Task<List<MealPlan>> GetByUserIdWithDetailsAsync(Guid userId)
    {
        return await _context.MealPlans
            .Include(mp => mp.Days)
                .ThenInclude(d => d.Meals)
                    .ThenInclude(m => m.Options)
            .Where(mp => mp.UserId == userId && mp.IsActive)
            .OrderByDescending(mp => mp.ImportedAt)
            .ToListAsync();
    }

    public async Task<MealPlanDay?> GetDayAsync(Guid mealPlanId, DateOnly date)
    {
        // Try exact date match first
        var day = await _context.MealPlanDays
            .Include(d => d.Meals)
                .ThenInclude(m => m.Options)
                    .ThenInclude(o => o.Ingredients)
            .FirstOrDefaultAsync(d => d.MealPlanId == mealPlanId && d.Date == date);

        if (day is not null) return day;

        // Fall back to matching by DayOfWeek (for template-based plans)
        var dayOfWeek = (int)date.DayOfWeek;
        return await _context.MealPlanDays
            .Include(d => d.Meals)
                .ThenInclude(m => m.Options)
                    .ThenInclude(o => o.Ingredients)
            .FirstOrDefaultAsync(d => d.MealPlanId == mealPlanId && d.DayOfWeek == dayOfWeek);
    }

    public async Task<MealOption?> GetMealOptionAsync(Guid optionId)
    {
        return await _context.Set<MealOption>()
            .FirstOrDefaultAsync(o => o.Id == optionId);
    }

    public async Task<List<MealOption>> GetSiblingOptionsAsync(Guid mealId)
    {
        return await _context.Set<MealOption>()
            .Where(o => o.MealId == mealId)
            .ToListAsync();
    }

    public async Task<Ingredient?> GetIngredientAsync(Guid ingredientId)
    {
        return await _context.Set<Ingredient>()
            .Include(i => i.MealOption)
            .FirstOrDefaultAsync(i => i.Id == ingredientId);
    }

    public void DeleteIngredient(Ingredient ingredient)
    {
        _context.Set<Ingredient>().Remove(ingredient);
    }

    public async Task DeactivateAllForUserAsync(Guid userId)
    {
        var plans = await _context.MealPlans
            .Where(mp => mp.UserId == userId && mp.IsActive)
            .ToListAsync();
        foreach (var plan in plans)
            plan.IsActive = false;
    }

    public async Task AddAsync(MealPlan mealPlan)
    {
        await _context.MealPlans.AddAsync(mealPlan);
    }

    public void Update(MealPlan mealPlan)
    {
        _context.MealPlans.Update(mealPlan);
    }

    public void UpdateOption(MealOption option)
    {
        _context.Set<MealOption>().Update(option);
    }
}
