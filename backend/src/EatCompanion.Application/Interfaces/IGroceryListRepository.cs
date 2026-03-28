using EatCompanion.Domain.Entities;

namespace EatCompanion.Application.Interfaces;

public interface IGroceryListRepository
{
    Task<GroceryList?> GetByIdWithItemsAsync(Guid id);
    Task<GroceryList?> GetLatestByMealPlanIdAsync(Guid mealPlanId);
    Task<GroceryItem?> GetItemByIdAsync(Guid itemId);
    Task AddAsync(GroceryList groceryList);
    void UpdateItem(GroceryItem item);
}
