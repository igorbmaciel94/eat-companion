using EatCompanion.Application.Common;
using EatCompanion.Application.DTOs;
using EatCompanion.Application.Interfaces;
using Mapster;

namespace EatCompanion.Application.UseCases.MealPlans;

public record GetDailySummaryQuery(Guid MealPlanId, DateOnly Date, Guid UserId);

public class GetDailySummaryQueryHandler
{
    private readonly IMealPlanRepository _mealPlanRepository;

    public GetDailySummaryQueryHandler(IMealPlanRepository mealPlanRepository)
    {
        _mealPlanRepository = mealPlanRepository;
    }

    public async Task<DailySummaryDto> Handle(GetDailySummaryQuery query)
    {
        var plan = await _mealPlanRepository.GetByIdAsync(query.MealPlanId);
        if (plan is null || plan.UserId != query.UserId)
            throw new NotFoundException("MealPlan", query.MealPlanId);

        var day = await _mealPlanRepository.GetDayAsync(query.MealPlanId, query.Date);
        if (day is null)
            throw new NotFoundException("MealPlanDay", query.Date);

        var meals = day.Meals.OrderBy(m => m.MealType).ThenBy(m => m.SortOrder).Adapt<List<MealDto>>();

        var selectedOptions = day.Meals
            .SelectMany(m => m.Options)
            .Where(o => o.IsSelected)
            .ToList();

        var totalCalories = selectedOptions.Sum(o => o.Calories ?? 0);
        var totalProtein = selectedOptions.Sum(o => o.ProteinGrams ?? 0);
        var totalCarbs = selectedOptions.Sum(o => o.CarbsGrams ?? 0);
        var totalFat = selectedOptions.Sum(o => o.FatGrams ?? 0);

        return new DailySummaryDto(
            query.Date,
            0,
            totalCalories,
            totalProtein,
            totalCarbs,
            totalFat,
            0,
            meals
        );
    }
}
