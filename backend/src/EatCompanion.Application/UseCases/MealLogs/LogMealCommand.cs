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
