using EatCompanion.Domain.Enums;

namespace EatCompanion.Application.DTOs;

public record UserProfileDto(
    Guid Id,
    string Email,
    string DisplayName,
    int CalorieTarget,
    GoalType GoalType,
    decimal? HeightCm,
    decimal? WeightKg,
    DateTime CreatedAt
);
