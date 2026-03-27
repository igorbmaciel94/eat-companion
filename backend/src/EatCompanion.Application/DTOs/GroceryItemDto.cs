using EatCompanion.Domain.Enums;

namespace EatCompanion.Application.DTOs;

public record GroceryItemDto(
    Guid Id,
    string Name,
    IngredientCategory Category,
    decimal TotalAmount,
    UnitOfMeasure Unit,
    bool IsChecked
);
