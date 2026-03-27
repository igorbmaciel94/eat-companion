using EatCompanion.Domain.Enums;

namespace EatCompanion.Application.DTOs;

public record IngredientDto(
    Guid Id,
    string Name,
    string NamePt,
    decimal Amount,
    UnitOfMeasure Unit,
    IngredientCategory Category
);
