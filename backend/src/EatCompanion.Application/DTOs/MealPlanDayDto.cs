namespace EatCompanion.Application.DTOs;

public record MealPlanDayDto(
    Guid Id,
    int DayOfWeek,
    DateOnly? Date,
    List<MealDto> Meals
);
