namespace EatCompanion.Application.DTOs;

public record DailySummaryDto(
    DateOnly Date,
    int CalorieTarget,
    int CaloriesConsumed,
    decimal ProteinConsumed,
    decimal CarbsConsumed,
    decimal FatConsumed,
    int MealsCompleted,
    List<MealDto> Meals
);
