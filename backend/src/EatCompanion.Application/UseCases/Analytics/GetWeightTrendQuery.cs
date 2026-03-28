using EatCompanion.Application.DTOs;
using EatCompanion.Application.Interfaces;
using Mapster;

namespace EatCompanion.Application.UseCases.Analytics;

public record GetWeightTrendQuery(Guid UserId, int Months);

public class GetWeightTrendQueryHandler
{
    private readonly IWeightEntryRepository _weightEntryRepository;

    public GetWeightTrendQueryHandler(IWeightEntryRepository weightEntryRepository)
    {
        _weightEntryRepository = weightEntryRepository;
    }

    public async Task<List<WeightEntryDto>> Handle(GetWeightTrendQuery query)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = today.AddMonths(-query.Months);

        var entries = await _weightEntryRepository.GetByUserAndDateRangeAsync(query.UserId, startDate, today);
        return entries.Adapt<List<WeightEntryDto>>();
    }
}
