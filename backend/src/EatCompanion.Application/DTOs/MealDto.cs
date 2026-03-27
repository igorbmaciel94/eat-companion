using EatCompanion.Domain.Enums;

namespace EatCompanion.Application.DTOs;

public record MealDto(
    Guid Id,
    MealType MealType,
    int SortOrder,
    List<MealOptionDto> Options
);
