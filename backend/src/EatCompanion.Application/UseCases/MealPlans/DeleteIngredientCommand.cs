using EatCompanion.Application.Common;
using EatCompanion.Domain.Interfaces;

namespace EatCompanion.Application.UseCases.MealPlans;

public record DeleteIngredientCommand(Guid PlanId, Guid IngredientId, Guid UserId);

public class DeleteIngredientCommandHandler
{
    private readonly IMealPlanRepository _mealPlanRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteIngredientCommandHandler(
        IMealPlanRepository mealPlanRepository,
        IUnitOfWork unitOfWork)
    {
        _mealPlanRepository = mealPlanRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteIngredientCommand command)
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

        _mealPlanRepository.DeleteIngredient(ingredient);
        await _unitOfWork.SaveChangesAsync();
    }
}
