using EatCompanion.Application.Common;
using EatCompanion.Application.DTOs;
using EatCompanion.Application.Interfaces;
using Mapster;

namespace EatCompanion.Application.UseCases.GroceryLists;

public record GetGroceryListQuery(Guid GroceryListId);

public class GetGroceryListQueryHandler
{
    private readonly IGroceryListRepository _groceryListRepository;

    public GetGroceryListQueryHandler(IGroceryListRepository groceryListRepository)
    {
        _groceryListRepository = groceryListRepository;
    }

    public async Task<GroceryListDto> Handle(GetGroceryListQuery query)
    {
        var list = await _groceryListRepository.GetByIdWithItemsAsync(query.GroceryListId);
        if (list is null)
            throw new NotFoundException("GroceryList", query.GroceryListId);

        return list.Adapt<GroceryListDto>();
    }
}
