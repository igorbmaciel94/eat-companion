using EatCompanion.Application.Common;
using EatCompanion.Domain.Interfaces;

namespace EatCompanion.Application.UseCases.MealLogs;

public record DeleteMealLogCommand(Guid UserId, DateOnly Date, Guid? MealOptionId);

public class DeleteMealLogCommandHandler
{
    private readonly IMealLogRepository _mealLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMealLogCommandHandler(
        IMealLogRepository mealLogRepository,
        IUnitOfWork unitOfWork)
    {
        _mealLogRepository = mealLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteMealLogCommand command)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (command.Date < today)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Date", ["Cannot modify logs for past dates."] }
            });

        var existing = await _mealLogRepository.GetByUserDateAndOptionAsync(
            command.UserId, command.Date, command.MealOptionId);

        if (existing is not null)
        {
            _mealLogRepository.Delete(existing);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
