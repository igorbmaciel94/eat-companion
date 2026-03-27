namespace EatCompanion.Domain.Entities;

public class MealPlan
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SourceFileName { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;
    public ICollection<MealPlanDay> Days { get; set; } = new List<MealPlanDay>();
    public ICollection<GroceryList> GroceryLists { get; set; } = new List<GroceryList>();
}
