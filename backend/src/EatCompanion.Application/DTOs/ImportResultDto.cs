namespace EatCompanion.Application.DTOs;

public record ImportResultDto(Guid MealPlanId, string Name, int TotalDays, int TotalMeals, int TotalOptions);
