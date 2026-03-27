using EatCompanion.Domain.Enums;

namespace EatCompanion.Domain.Entities;

public class ImportJob
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string TempFilePath { get; set; } = string.Empty;
    public ImportJobStatus Status { get; set; } = ImportJobStatus.Pending;
    public Guid? MealPlanId { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public User User { get; set; } = null!;
}
