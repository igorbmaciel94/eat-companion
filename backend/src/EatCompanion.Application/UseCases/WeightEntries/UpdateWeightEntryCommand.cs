using EatCompanion.Application.Common;
using EatCompanion.Application.DTOs;
using EatCompanion.Domain.Interfaces;
using Mapster;

namespace EatCompanion.Application.UseCases.WeightEntries;

public record UpdateWeightEntryCommand(Guid UserId, Guid EntryId, decimal WeightKg, string? Notes);

public class UpdateWeightEntryCommandHandler
{
    private readonly IWeightEntryRepository _weightEntryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateWeightEntryCommandHandler(
        IWeightEntryRepository weightEntryRepository,
        IUnitOfWork unitOfWork)
    {
        _weightEntryRepository = weightEntryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WeightEntryDto?> Handle(UpdateWeightEntryCommand command)
    {
        var entry = await _weightEntryRepository.GetByIdAsync(command.EntryId);
        if (entry is null || entry.UserId != command.UserId)
            return null;

        entry.WeightKg = command.WeightKg;
        entry.Notes = command.Notes;
        _weightEntryRepository.Update(entry);
        await _unitOfWork.SaveChangesAsync();

        return entry.Adapt<WeightEntryDto>();
    }
}
