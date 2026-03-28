using EatCompanion.Application.DTOs;
using EatCompanion.Application.Interfaces;
using Mapster;

namespace EatCompanion.Application.UseCases.GroceryLists;

public record GetLatestGroceryListQuery(Guid MealPlanId);

public class GetLatestGroceryListQueryHandler
{
    private readonly IGroceryListRepository _groceryListRepository;

    public GetLatestGroceryListQueryHandler(IGroceryListRepository groceryListRepository)
    {
        _groceryListRepository = groceryListRepository;
    }

    public async Task<GroceryListDto?> Handle(GetLatestGroceryListQuery query)
    {
        var list = await _groceryListRepository.GetLatestByMealPlanIdAsync(query.MealPlanId);
        if (list is null)
            return null;

        return list.Adapt<GroceryListDto>();
    }
}
