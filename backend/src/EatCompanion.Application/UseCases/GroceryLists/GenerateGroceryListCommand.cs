using EatCompanion.Application.Common;
using EatCompanion.Application.DTOs;
using EatCompanion.Application.Interfaces;
using EatCompanion.Domain.Interfaces;

namespace EatCompanion.Application.UseCases.GroceryLists;

public record GenerateGroceryListCommand(Guid MealPlanId, DateOnly StartDate, DateOnly EndDate, Guid UserId);

public class GenerateGroceryListCommandHandler
{
    private readonly IMealPlanRepository _mealPlanRepo;
    private readonly IGroceryListGenerator _generator;
    private readonly IGroceryListRepository _groceryListRepo;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateGroceryListCommandHandler(
        IMealPlanRepository mealPlanRepo,
        IGroceryListGenerator generator,
        IGroceryListRepository groceryListRepo,
        IUnitOfWork unitOfWork)
    {
        _mealPlanRepo = mealPlanRepo;
        _generator = generator;
        _groceryListRepo = groceryListRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<GroceryListDto> Handle(GenerateGroceryListCommand command, CancellationToken cancellationToken = default)
    {
        var mealPlan = await _mealPlanRepo.GetByIdAsync(command.MealPlanId);
        if (mealPlan is null)
            throw new InvalidOperationException($"Meal plan {command.MealPlanId} not found.");

        var groceryList = _generator.Generate(mealPlan, command.StartDate, command.EndDate, command.UserId);

        await _groceryListRepo.AddAsync(groceryList);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new GroceryListDto(
            groceryList.Id,
            groceryList.MealPlanId,
            groceryList.StartDate,
            groceryList.EndDate,
            groceryList.CreatedAt,
            groceryList.Items.Select(i => new GroceryItemDto(
                i.Id,
                i.Name,
                i.NamePt,
                i.Category,
                i.TotalAmount,
                i.Unit,
                i.IsChecked
            )).ToList()
        );
    }
}
