namespace EatCompanion.Domain.Entities;

public class GroceryList
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid MealPlanId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public User User { get; set; } = null!;
    public MealPlan MealPlan { get; set; } = null!;
    public ICollection<GroceryItem> Items { get; set; } = new List<GroceryItem>();
}
