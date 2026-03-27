using EatCompanion.Domain.Enums;

namespace EatCompanion.Infrastructure.PdfParsing;

public record ParsedMealPlan(List<ParsedMeal> Meals);

public record ParsedMeal(MealType Type, List<ParsedMealOption> Options);

public record ParsedMealOption(string Description, List<ParsedIngredient> Ingredients);

public record ParsedIngredient(string NamePt, decimal Amount, UnitOfMeasure Unit);

internal enum SectionType
{
    Breakfast,
    Lunch,
    Snack,
    Dinner,
    Recommendations,
    Recipe
}

internal record DetectedSection(SectionType Type, MealType? MealType, string Text);
