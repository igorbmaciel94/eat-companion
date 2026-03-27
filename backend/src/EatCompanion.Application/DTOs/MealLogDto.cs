using EatCompanion.Domain.Enums;

namespace EatCompanion.Application.DTOs;

public record MealLogDto(
    Guid Id,
    DateOnly Date,
    MealType MealType,
    MealLogStatus Status,
    Guid? MealOptionId,
    string? Notes,
    DateTime CreatedAt
);
