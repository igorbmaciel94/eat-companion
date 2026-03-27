namespace EatCompanion.Domain.Entities;

public class GroceryList
{
    public Guid Id { get; set; }
    public Guid MealPlanId { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public MealPlan MealPlan { get; set; } = null!;
    public ICollection<GroceryItem> Items { get; set; } = new List<GroceryItem>();
}
