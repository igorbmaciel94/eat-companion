using EatCompanion.Application.Common;
using EatCompanion.Domain.Enums;
using EatCompanion.Domain.Interfaces;

namespace EatCompanion.Application.UseCases.MealPlans;

public record UpdateIngredientCommand(
    Guid PlanId,
    Guid IngredientId,
    string Name,
    string? NamePt,
    decimal Amount,
    UnitOfMeasure Unit,
    IngredientCategory Category,
    Guid UserId);

public class UpdateIngredientCommandHandler
{
    private readonly IMealPlanRepository _mealPlanRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateIngredientCommandHandler(
        IMealPlanRepository mealPlanRepository,
        IUnitOfWork unitOfWork)
    {
        _mealPlanRepository = mealPlanRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateIngredientCommand command)
    {
        var plan = await _mealPlanRepository.GetByIdAsync(command.PlanId);
        if (plan is null || plan.UserId != command.UserId)
            throw new NotFoundException("MealPlan", command.PlanId);

        var ingredient = await _mealPlanRepository.GetIngredientAsync(command.IngredientId);
        if (ingredient is null)
            throw new NotFoundException("Ingredient", command.IngredientId);

        // Verify the ingredient belongs to this plan
        var belongsToPlan = plan.Days
            .SelectMany(d => d.Meals)
            .SelectMany(m => m.Options)
            .Any(o => o.Id == ingredient.MealOptionId);

        if (!belongsToPlan)
            throw new NotFoundException("Ingredient", command.IngredientId);

        ingredient.Name = command.Name;
        ingredient.NamePt = command.NamePt;
        ingredient.Amount = command.Amount;
        ingredient.Quantity = command.Amount;
        ingredient.Unit = command.Unit;
        ingredient.Category = command.Category;

        await _unitOfWork.SaveChangesAsync();
    }
}
