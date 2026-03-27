namespace EatCompanion.Application.DTOs;

public record MealOptionDto(
    Guid Id,
    string? Name,
    string Description,
    bool IsSelected,
    int SortOrder,
    int? Calories,
    decimal? ProteinGrams,
    decimal? CarbsGrams,
    decimal? FatGrams,
    List<IngredientDto> Ingredients
);
