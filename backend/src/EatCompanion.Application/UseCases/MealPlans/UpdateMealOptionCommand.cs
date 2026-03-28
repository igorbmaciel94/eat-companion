using EatCompanion.Application.Common;
using EatCompanion.Application.Interfaces;

namespace EatCompanion.Application.UseCases.MealPlans;

public record UpdateMealOptionCommand(
    Guid PlanId,
    Guid OptionId,
    string? Name,
    string? Description,
    int? Calories,
    decimal? ProteinGrams,
    decimal? CarbsGrams,
    decimal? FatGrams,
    Guid UserId);

public class UpdateMealOptionCommandHandler
{
    private readonly IMealPlanRepository _mealPlanRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMealOptionCommandHandler(
        IMealPlanRepository mealPlanRepository,
        IUnitOfWork unitOfWork)
    {
        _mealPlanRepository = mealPlanRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateMealOptionCommand command)
    {
        var plan = await _mealPlanRepository.GetByIdAsync(command.PlanId);
        if (plan is null || plan.UserId != command.UserId)
            throw new NotFoundException("MealPlan", command.PlanId);

        // Find all options with the same Name across all days (clones)
        var targetOption = plan.Days
            .SelectMany(d => d.Meals)
            .SelectMany(m => m.Options)
            .FirstOrDefault(o => o.Id == command.OptionId);

        if (targetOption is null)
            throw new NotFoundException("MealOption", command.OptionId);

        // Update the option on ALL days (since they're clones with the same name)
        var matchingOptions = plan.Days
            .SelectMany(d => d.Meals)
            .SelectMany(m => m.Options)
            .Where(o => o.Name == targetOption.Name);

        foreach (var option in matchingOptions)
        {
            if (command.Name is not null)
                option.Name = command.Name;
            if (command.Description is not null)
                option.Description = command.Description;
            if (command.Calories.HasValue)
                option.Calories = command.Calories;
            if (command.ProteinGrams.HasValue)
                option.ProteinGrams = command.ProteinGrams;
            if (command.CarbsGrams.HasValue)
                option.CarbsGrams = command.CarbsGrams;
            if (command.FatGrams.HasValue)
                option.FatGrams = command.FatGrams;

            _mealPlanRepository.UpdateOption(option);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
