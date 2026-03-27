namespace EatCompanion.Domain.Entities;

public class MealOption
{
    public Guid Id { get; set; }
    public Guid MealId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Calories { get; set; }
    public decimal? ProteinGrams { get; set; }
    public decimal? CarbsGrams { get; set; }
    public decimal? FatGrams { get; set; }
    public bool IsSelected { get; set; }
    public int SortOrder { get; set; }

    public Meal Meal { get; set; } = null!;
    public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
}
