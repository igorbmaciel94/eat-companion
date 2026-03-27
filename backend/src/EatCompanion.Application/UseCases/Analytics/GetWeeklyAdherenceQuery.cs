using EatCompanion.Application.DTOs;
using EatCompanion.Domain.Enums;
using EatCompanion.Domain.Interfaces;

namespace EatCompanion.Application.UseCases.Analytics;

public record GetWeeklyAdherenceQuery(Guid UserId, int Weeks);

public class GetWeeklyAdherenceQueryHandler
{
    private readonly IMealLogRepository _mealLogRepository;
    private readonly IMealPlanRepository _mealPlanRepository;

    public GetWeeklyAdherenceQueryHandler(
        IMealLogRepository mealLogRepository,
        IMealPlanRepository mealPlanRepository)
    {
        _mealLogRepository = mealLogRepository;
        _mealPlanRepository = mealPlanRepository;
    }

    public async Task<List<WeeklyAnalyticsDto>> Handle(GetWeeklyAdherenceQuery query)
    {
        var results = new List<WeeklyAnalyticsDto>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        for (int i = 0; i < query.Weeks; i++)
        {
            var weekEnd = today.AddDays(-7 * i);
            var weekStart = weekEnd.AddDays(-6);

            var plans = await _mealPlanRepository.GetByUserIdAsync(query.UserId);
            var plannedMeals = plans
                .SelectMany(p => p.Days)
                .Where(d => d.Date >= weekStart && d.Date <= weekEnd)
                .SelectMany(d => d.Meals)
                .Count();

            var logs = await _mealLogRepository.GetByUserAndDateRangeAsync(query.UserId, weekStart, weekEnd);
            var loggedMeals = logs.Count(l => l.Status == MealLogStatus.Completed);

            var adherence = plannedMeals > 0
                ? Math.Round((decimal)loggedMeals / plannedMeals * 100, 1)
                : 0;

            var completedLogs = logs.Where(l => l.Status == MealLogStatus.Completed).ToList();
            var skippedCount = logs.Count(l => l.Status == MealLogStatus.Skipped);
            var substitutedCount = logs.Count(l => l.Status == MealLogStatus.Substituted);

            results.Add(new WeeklyAnalyticsDto(
                weekStart,
                weekEnd,
                plannedMeals > 0 ? (int)Math.Round((decimal)completedLogs.Count / plannedMeals * 2000) : 0,
                0m,
                0m,
                0m,
                loggedMeals,
                skippedCount,
                substitutedCount,
                new List<DailySummaryDto>()
            ));
        }

        return results;
    }
}
