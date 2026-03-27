namespace EatCompanion.Application.DTOs;

public record WeeklyAnalyticsDto(
    DateOnly StartDate,
    DateOnly EndDate,
    int AverageCalories,
    decimal AverageProtein,
    decimal AverageCarbs,
    decimal AverageFat,
    int MealsCompleted,
    int MealsSkipped,
    int MealsSubstituted,
    List<DailySummaryDto> DailySummaries
);
