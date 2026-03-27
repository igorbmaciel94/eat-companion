using EatCompanion.Application.DTOs;
using EatCompanion.Domain.Interfaces;
using Mapster;

namespace EatCompanion.Application.UseCases.MealPlans;

public record GetMealPlansQuery(Guid UserId);

public class GetMealPlansQueryHandler
{
    private readonly IMealPlanRepository _mealPlanRepository;

    public GetMealPlansQueryHandler(IMealPlanRepository mealPlanRepository)
    {
        _mealPlanRepository = mealPlanRepository;
    }

    public async Task<List<MealPlanDto>> Handle(GetMealPlansQuery query)
    {
        var plans = await _mealPlanRepository.GetByUserIdAsync(query.UserId);
        return plans.Adapt<List<MealPlanDto>>();
    }
}
