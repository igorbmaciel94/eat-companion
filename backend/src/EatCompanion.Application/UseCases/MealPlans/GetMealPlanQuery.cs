using EatCompanion.Application.Common;
using EatCompanion.Application.DTOs;
using EatCompanion.Application.Interfaces;
using Mapster;

namespace EatCompanion.Application.UseCases.MealPlans;

public record GetMealPlanQuery(Guid MealPlanId, Guid UserId);

public class GetMealPlanQueryHandler
{
    private readonly IMealPlanRepository _mealPlanRepository;

    public GetMealPlanQueryHandler(IMealPlanRepository mealPlanRepository)
    {
        _mealPlanRepository = mealPlanRepository;
    }

    public async Task<MealPlanDto> Handle(GetMealPlanQuery query)
    {
        var plan = await _mealPlanRepository.GetByIdWithDetailsAsync(query.MealPlanId);
        if (plan is null || plan.UserId != query.UserId)
            throw new NotFoundException("MealPlan", query.MealPlanId);

        return plan.Adapt<MealPlanDto>();
    }
}
