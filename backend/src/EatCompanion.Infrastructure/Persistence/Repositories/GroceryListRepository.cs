using EatCompanion.Domain.Entities;
using EatCompanion.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EatCompanion.Infrastructure.Persistence.Repositories;

public class GroceryListRepository : IGroceryListRepository
{
    private readonly AppDbContext _context;

    public GroceryListRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GroceryList?> GetByIdAsync(Guid id)
    {
        return await _context.GroceryLists
            .Include(gl => gl.Items)
            .FirstOrDefaultAsync(gl => gl.Id == id);
    }

    public async Task<IReadOnlyList<GroceryList>> GetByUserIdAsync(Guid userId)
    {
        return await _context.GroceryLists
            .Include(gl => gl.Items)
            .Where(gl => gl.UserId == userId)
            .OrderByDescending(gl => gl.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(GroceryList groceryList)
    {
        await _context.GroceryLists.AddAsync(groceryList);
    }

    public void Update(GroceryList groceryList)
    {
        _context.GroceryLists.Update(groceryList);
    }
}
