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
        var plans = await _mealPlanRepository.GetByUserIdWithDetailsAsync(query.UserId);

        for (int i = 0; i < query.Weeks; i++)
        {
            var weekEnd = today.AddDays(-7 * i);
            var weekStart = weekEnd.AddDays(-6);

            var logs = await _mealLogRepository.GetByUserAndDateRangeAsync(query.UserId, weekStart, weekEnd);
            var loggedMeals = logs.Count(l => l.Status == MealLogStatus.Completed);
            var skippedCount = logs.Count(l => l.Status == MealLogStatus.Skipped);
            var substitutedCount = logs.Count(l => l.Status == MealLogStatus.Substituted);

            // Build daily summaries
            var dailySummaries = new List<DailySummaryDto>();
            for (var date = weekStart; date <= weekEnd; date = date.AddDays(1))
            {
                var dayLogs = logs.Where(l => l.Date == date).ToList();
                var completedDayLogs = dayLogs.Where(l => l.Status == MealLogStatus.Completed).ToList();

                // Sum calories from completed meal options
                var caloriesConsumed = 0;
                decimal proteinConsumed = 0, carbsConsumed = 0, fatConsumed = 0;
                foreach (var log in completedDayLogs)
                {
                    if (log.MealOptionId == null) continue;
                    var option = plans
                        .SelectMany(p => p.Days)
                        .SelectMany(d => d.Meals)
                        .SelectMany(m => m.Options)
                        .FirstOrDefault(o => o.Id == log.MealOptionId);
                    if (option != null)
                    {
                        caloriesConsumed += option.Calories ?? 0;
                        proteinConsumed += option.ProteinGrams ?? 0;
                        carbsConsumed += option.CarbsGrams ?? 0;
                        fatConsumed += option.FatGrams ?? 0;
                    }
                }

                dailySummaries.Add(new DailySummaryDto(
                    date,
                    2000, // default target
                    caloriesConsumed,
                    proteinConsumed,
                    carbsConsumed,
                    fatConsumed,
                    completedDayLogs.Count,
                    new List<MealDto>()
                ));
            }

            var totalCalories = dailySummaries.Sum(d => d.CaloriesConsumed);
            var daysWithData = dailySummaries.Count(d => d.CaloriesConsumed > 0);
            var avgCalories = daysWithData > 0 ? totalCalories / daysWithData : 0;

            results.Add(new WeeklyAnalyticsDto(
                weekStart,
                weekEnd,
                avgCalories,
                dailySummaries.Sum(d => d.ProteinConsumed) / Math.Max(daysWithData, 1),
                dailySummaries.Sum(d => d.CarbsConsumed) / Math.Max(daysWithData, 1),
                dailySummaries.Sum(d => d.FatConsumed) / Math.Max(daysWithData, 1),
                loggedMeals,
                skippedCount,
                substitutedCount,
                dailySummaries
            ));
        }

        return results;
    }
}
