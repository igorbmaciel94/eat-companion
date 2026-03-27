using EatCompanion.Application.Common;
using EatCompanion.Domain.Interfaces;

namespace EatCompanion.Application.UseCases.MealPlans;

public record SelectMealOptionCommand(Guid MealPlanId, Guid MealId, Guid OptionId, Guid UserId);

public class SelectMealOptionCommandHandler
{
    private readonly IMealPlanRepository _mealPlanRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SelectMealOptionCommandHandler(
        IMealPlanRepository mealPlanRepository,
        IUnitOfWork unitOfWork)
    {
        _mealPlanRepository = mealPlanRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SelectMealOptionCommand command)
    {
        var plan = await _mealPlanRepository.GetByIdAsync(command.MealPlanId);
        if (plan is null || plan.UserId != command.UserId)
            throw new NotFoundException("MealPlan", command.MealPlanId);

        var option = await _mealPlanRepository.GetMealOptionAsync(command.OptionId);
        if (option is null || option.MealId != command.MealId)
            throw new NotFoundException("MealOption", command.OptionId);

        var siblings = await _mealPlanRepository.GetSiblingOptionsAsync(command.MealId);
        foreach (var sibling in siblings)
        {
            sibling.IsSelected = sibling.Id == command.OptionId;
            _mealPlanRepository.UpdateOption(sibling);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
