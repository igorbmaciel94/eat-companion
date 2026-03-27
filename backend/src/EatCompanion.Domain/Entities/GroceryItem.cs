using EatCompanion.Domain.Enums;

namespace EatCompanion.Domain.Entities;

public class GroceryItem
{
    public Guid Id { get; set; }
    public Guid GroceryListId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public UnitOfMeasure Unit { get; set; }
    public IngredientCategory Category { get; set; }
    public bool IsChecked { get; set; }

    public GroceryList GroceryList { get; set; } = null!;
}
