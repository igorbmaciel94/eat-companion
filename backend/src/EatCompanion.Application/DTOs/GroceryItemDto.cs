using EatCompanion.Domain.Enums;

namespace EatCompanion.Application.DTOs;

public record GroceryItemDto(
    Guid Id,
    string Name,
    string? NamePt,
    IngredientCategory Category,
    decimal TotalAmount,
    UnitOfMeasure Unit,
    bool IsChecked
);
