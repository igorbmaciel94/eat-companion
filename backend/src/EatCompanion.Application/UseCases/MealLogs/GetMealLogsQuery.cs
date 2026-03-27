using EatCompanion.Application.DTOs;
using EatCompanion.Domain.Interfaces;
using Mapster;

namespace EatCompanion.Application.UseCases.MealLogs;

public record GetMealLogsQuery(Guid UserId, DateOnly StartDate, DateOnly EndDate);

public class GetMealLogsQueryHandler
{
    private readonly IMealLogRepository _mealLogRepository;

    public GetMealLogsQueryHandler(IMealLogRepository mealLogRepository)
    {
        _mealLogRepository = mealLogRepository;
    }

    public async Task<List<MealLogDto>> Handle(GetMealLogsQuery query)
    {
        var logs = await _mealLogRepository.GetByUserAndDateRangeAsync(query.UserId, query.StartDate, query.EndDate);
        return logs.Adapt<List<MealLogDto>>();
    }
}
