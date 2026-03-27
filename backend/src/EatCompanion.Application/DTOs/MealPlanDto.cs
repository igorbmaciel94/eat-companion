namespace EatCompanion.Application.DTOs;

public record MealPlanDto(
    Guid Id,
    string Name,
    string? SourceFileName,
    DateTime ImportedAt,
    bool IsActive,
    List<MealPlanDayDto> Days
);
