using EatCompanion.Domain.Enums;

namespace EatCompanion.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int CalorieTarget { get; set; } = 2000;
    public GoalType GoalType { get; set; } = GoalType.Maintain;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MealPlan> MealPlans { get; set; } = new List<MealPlan>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<MealLog> MealLogs { get; set; } = new List<MealLog>();
    public ICollection<WeightEntry> WeightEntries { get; set; } = new List<WeightEntry>();
    public ICollection<GroceryList> GroceryLists { get; set; } = new List<GroceryList>();
}
