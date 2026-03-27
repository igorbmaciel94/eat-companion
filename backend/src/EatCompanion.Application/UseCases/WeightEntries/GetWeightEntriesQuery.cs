using EatCompanion.Application.DTOs;
using EatCompanion.Domain.Interfaces;
using Mapster;

namespace EatCompanion.Application.UseCases.WeightEntries;

public record GetWeightEntriesQuery(Guid UserId, DateOnly StartDate, DateOnly EndDate);

public class GetWeightEntriesQueryHandler
{
    private readonly IWeightEntryRepository _weightEntryRepository;

    public GetWeightEntriesQueryHandler(IWeightEntryRepository weightEntryRepository)
    {
        _weightEntryRepository = weightEntryRepository;
    }

    public async Task<List<WeightEntryDto>> Handle(GetWeightEntriesQuery query)
    {
        var entries = await _weightEntryRepository.GetByUserAndDateRangeAsync(query.UserId, query.StartDate, query.EndDate);
        return entries.Adapt<List<WeightEntryDto>>();
    }
}
