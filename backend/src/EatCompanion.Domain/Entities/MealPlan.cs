namespace EatCompanion.Domain.Entities;

public class MealPlan
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SourceFileName { get; set; }
    public DateTime ImportedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public User User { get; set; } = null!;
    public ICollection<MealPlanDay> Days { get; set; } = new List<MealPlanDay>();
}
