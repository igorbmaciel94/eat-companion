namespace EatCompanion.Application.DTOs;

public record GroceryListDto(
    Guid Id,
    Guid MealPlanId,
    DateOnly StartDate,
    DateOnly EndDate,
    DateTime CreatedAt,
    List<GroceryItemDto> Items
);
