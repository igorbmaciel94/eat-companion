namespace EatCompanion.Domain.Entities;

public class WeightEntry
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly Date { get; set; }
    public decimal WeightKg { get; set; }
    public DateTime CreatedAt { get; set; }
    public User User { get; set; } = null!;
}
