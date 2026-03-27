using EatCompanion.Application.DTOs;
using EatCompanion.Application.Common;
using EatCompanion.Domain.Entities;
using EatCompanion.Domain.Enums;
using EatCompanion.Domain.Interfaces;
using Mapster;

namespace EatCompanion.Application.UseCases.MealLogs;

public record LogMealCommand(
    Guid UserId,
    DateOnly Date,
    MealType MealType,
    MealLogStatus Status,
    Guid? MealOptionId,
    string? Notes
);

public class LogMealCommandHandler
{
    private readonly IMealLogRepository _mealLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogMealCommandHandler(
        IMealLogRepository mealLogRepository,
        IUnitOfWork unitOfWork)
    {
        _mealLogRepository = mealLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MealLogDto> Handle(LogMealCommand command)
    {
        // Block logging for past dates
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (command.Date < today)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Date", ["Cannot modify logs for past dates."] }
            });

        // Upsert: if log for same user+date+option exists, update it
        var existing = await _mealLogRepository.GetByUserDateAndOptionAsync(
            command.UserId, command.Date, command.MealOptionId);

        if (existing is not null)
        {
            existing.Status = command.Status;
            existing.Notes = command.Notes;
            _mealLogRepository.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            return existing.Adapt<MealLogDto>();
        }

        var mealLog = new MealLog
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            Date = command.Date,
            MealType = command.MealType,
            Status = command.Status,
            MealOptionId = command.MealOptionId,
            Notes = command.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _mealLogRepository.AddAsync(mealLog);
        await _unitOfWork.SaveChangesAsync();

        return mealLog.Adapt<MealLogDto>();
    }
}
