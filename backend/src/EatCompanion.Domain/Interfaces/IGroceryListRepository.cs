using EatCompanion.Domain.Entities;

namespace EatCompanion.Domain.Interfaces;

public interface IGroceryListRepository
{
    Task<GroceryList?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<GroceryList>> GetByUserIdAsync(Guid userId);
    Task AddAsync(GroceryList groceryList);
    void Update(GroceryList groceryList);
}
