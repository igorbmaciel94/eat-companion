using EatCompanion.Domain.Enums;

namespace EatCompanion.Domain.Entities;

public class MealLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly Date { get; set; }
    public MealType MealType { get; set; }
    public MealLogStatus Status { get; set; }
    public Guid? MealOptionId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public MealOption? MealOption { get; set; }
}
