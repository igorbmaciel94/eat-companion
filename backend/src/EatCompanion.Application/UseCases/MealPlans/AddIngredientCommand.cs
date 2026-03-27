using EatCompanion.Application.Common;
using EatCompanion.Domain.Entities;
using EatCompanion.Domain.Enums;
using EatCompanion.Domain.Interfaces;

namespace EatCompanion.Application.UseCases.MealPlans;

public record AddIngredientCommand(
    Guid PlanId,
    Guid OptionId,
    string Name,
    string? NamePt,
    decimal Amount,
    UnitOfMeasure Unit,
    IngredientCategory Category,
    Guid UserId);

public class AddIngredientCommandHandler
{
    private readonly IMealPlanRepository _mealPlanRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddIngredientCommandHandler(
        IMealPlanRepository mealPlanRepository,
        IUnitOfWork unitOfWork)
    {
        _mealPlanRepository = mealPlanRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AddIngredientCommand command)
    {
        var plan = await _mealPlanRepository.GetByIdAsync(command.PlanId);
        if (plan is null || plan.UserId != command.UserId)
            throw new NotFoundException("MealPlan", command.PlanId);

        var targetOption = plan.Days
            .SelectMany(d => d.Meals)
            .SelectMany(m => m.Options)
            .FirstOrDefault(o => o.Id == command.OptionId);

        if (targetOption is null)
            throw new NotFoundException("MealOption", command.OptionId);

        // Add ingredient to this option on ALL days (clones with same name)
        var matchingOptions = plan.Days
            .SelectMany(d => d.Meals)
            .SelectMany(m => m.Options)
            .Where(o => o.Name == targetOption.Name);

        foreach (var option in matchingOptions)
        {
            var ingredient = new Ingredient
            {
                Id = Guid.NewGuid(),
                MealOptionId = option.Id,
                Name = command.Name,
                NamePt = command.NamePt,
                Amount = command.Amount,
                Quantity = command.Amount,
                Unit = command.Unit,
                Category = command.Category
            };

            option.Ingredients.Add(ingredient);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
