using EatCompanion.Application.Common;
using EatCompanion.Application.DTOs;
using EatCompanion.Domain.Interfaces;
using Mapster;

namespace EatCompanion.Application.UseCases.MealPlans;

public record GetWeeklySummaryQuery(Guid MealPlanId, Guid UserId);

public class GetWeeklySummaryQueryHandler
{
    private readonly IMealPlanRepository _mealPlanRepository;

    public GetWeeklySummaryQueryHandler(IMealPlanRepository mealPlanRepository)
    {
        _mealPlanRepository = mealPlanRepository;
    }

    public async Task<List<DailySummaryDto>> Handle(GetWeeklySummaryQuery query)
    {
        var plan = await _mealPlanRepository.GetByIdAsync(query.MealPlanId);
        if (plan is null || plan.UserId != query.UserId)
            throw new NotFoundException("MealPlan", query.MealPlanId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        // Get the Monday of the current week
        var dayOfWeek = today.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)today.DayOfWeek - 1;
        var monday = today.AddDays(-dayOfWeek);

        var result = new List<DailySummaryDto>();

        for (int i = 0; i < 7; i++)
        {
            var date = monday.AddDays(i);
            var day = await _mealPlanRepository.GetDayAsync(query.MealPlanId, date);

            if (day is null)
            {
                result.Add(new DailySummaryDto(date, 0, 0, 0, 0, 0, 0, new List<MealDto>()));
                continue;
            }

            var meals = day.Meals.OrderBy(m => m.MealType).ThenBy(m => m.SortOrder).Adapt<List<MealDto>>();

            var selectedOptions = day.Meals
                .SelectMany(m => m.Options)
                .Where(o => o.IsSelected)
                .ToList();

            var totalCalories = selectedOptions.Sum(o => o.Calories ?? 0);
            var totalProtein = selectedOptions.Sum(o => o.ProteinGrams ?? 0);
            var totalCarbs = selectedOptions.Sum(o => o.CarbsGrams ?? 0);
            var totalFat = selectedOptions.Sum(o => o.FatGrams ?? 0);

            result.Add(new DailySummaryDto(
                date, 0, totalCalories, totalProtein, totalCarbs, totalFat, 0, meals
            ));
        }

        return result;
    }
}
