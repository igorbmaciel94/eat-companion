using EatCompanion.Domain.Enums;

namespace EatCompanion.Domain.Entities;

public class Meal
{
    public Guid Id { get; set; }
    public Guid MealPlanDayId { get; set; }
    public MealType MealType { get; set; }
    public int SortOrder { get; set; }
    public MealPlanDay MealPlanDay { get; set; } = null!;
    public ICollection<MealOption> Options { get; set; } = new List<MealOption>();
}
