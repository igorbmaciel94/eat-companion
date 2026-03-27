using EatCompanion.Domain.Enums;

namespace EatCompanion.Domain.Entities;

public class Ingredient
{
    public Guid Id { get; set; }
    public Guid MealOptionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NamePt { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public UnitOfMeasure Unit { get; set; }
    public IngredientCategory Category { get; set; }
    public MealOption MealOption { get; set; } = null!;
}
