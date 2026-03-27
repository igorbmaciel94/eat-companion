using EatCompanion.Application.Common;
using EatCompanion.Domain.Interfaces;

namespace EatCompanion.Application.UseCases.GroceryLists;

public record ToggleGroceryItemCommand(Guid GroceryListId, Guid ItemId);

public class ToggleGroceryItemCommandHandler
{
    private readonly IGroceryListRepository _groceryListRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ToggleGroceryItemCommandHandler(
        IGroceryListRepository groceryListRepository,
        IUnitOfWork unitOfWork)
    {
        _groceryListRepository = groceryListRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ToggleGroceryItemCommand command)
    {
        var item = await _groceryListRepository.GetItemByIdAsync(command.ItemId);
        if (item is null || item.GroceryListId != command.GroceryListId)
            throw new NotFoundException("GroceryItem", command.ItemId);

        item.IsChecked = !item.IsChecked;
        _groceryListRepository.UpdateItem(item);
        await _unitOfWork.SaveChangesAsync();
    }
}
