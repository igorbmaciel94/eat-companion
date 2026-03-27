using EatCompanion.Application.DTOs;
using EatCompanion.Domain.Entities;

namespace EatCompanion.Application.Interfaces;

public interface INutritionCalculator
{
    DailySummaryDto CalculateDailySummary(MealPlanDay day, IEnumerable<MealLog> logs, int calorieTarget);
}
