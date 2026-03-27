using EatCompanion.Application.Common;
using EatCompanion.Application.DTOs;
using EatCompanion.Domain.Entities;
using EatCompanion.Domain.Interfaces;
using Mapster;

namespace EatCompanion.Application.UseCases.WeightEntries;

public record AddWeightEntryCommand(Guid UserId, DateOnly Date, decimal WeightKg, string? Notes);

public class AddWeightEntryCommandHandler
{
    private readonly IWeightEntryRepository _weightEntryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddWeightEntryCommandHandler(
        IWeightEntryRepository weightEntryRepository,
        IUnitOfWork unitOfWork)
    {
        _weightEntryRepository = weightEntryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WeightEntryDto> Handle(AddWeightEntryCommand command)
    {
        // Upsert: if entry for same user+date exists, update it
        var existing = await _weightEntryRepository.GetByUserAndDateAsync(command.UserId, command.Date);
        if (existing is not null)
        {
            existing.WeightKg = command.WeightKg;
            existing.Notes = command.Notes;
            _weightEntryRepository.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            return existing.Adapt<WeightEntryDto>();
        }

        var entry = new WeightEntry
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            Date = command.Date,
            WeightKg = command.WeightKg,
            Notes = command.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _weightEntryRepository.AddAsync(entry);
        await _unitOfWork.SaveChangesAsync();

        return entry.Adapt<WeightEntryDto>();
    }
}
