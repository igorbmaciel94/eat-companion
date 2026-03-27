namespace EatCompanion.Domain.Entities;

public class MealPlanDay
{
    public Guid Id { get; set; }
    public Guid MealPlanId { get; set; }
    public DateOnly Date { get; set; }
    public int DayOfWeek { get; set; }

    public MealPlan MealPlan { get; set; } = null!;
    public ICollection<Meal> Meals { get; set; } = new List<Meal>();
}
