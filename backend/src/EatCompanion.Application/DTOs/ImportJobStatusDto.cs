using EatCompanion.Domain.Enums;

namespace EatCompanion.Application.DTOs;

public record ImportJobStatusDto(
    Guid JobId,
    ImportJobStatus Status,
    Guid? MealPlanId,
    string? ErrorMessage
);
