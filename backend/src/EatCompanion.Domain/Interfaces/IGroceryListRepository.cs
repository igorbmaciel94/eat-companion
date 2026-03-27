using EatCompanion.Domain.Entities;

namespace EatCompanion.Domain.Interfaces;

public interface IGroceryListRepository
{
    Task<GroceryList?> GetByIdWithItemsAsync(Guid id);
    Task<GroceryItem?> GetItemByIdAsync(Guid itemId);
    Task AddAsync(GroceryList groceryList);
    void UpdateItem(GroceryItem item);
}
