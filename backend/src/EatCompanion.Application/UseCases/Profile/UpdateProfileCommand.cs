using EatCompanion.Application.Common;
using EatCompanion.Application.DTOs;
using EatCompanion.Domain.Enums;
using EatCompanion.Domain.Interfaces;

namespace EatCompanion.Application.UseCases.Profile;

public record UpdateProfileCommand(Guid UserId, string? DisplayName, int? CalorieTarget, GoalType? GoalType);

public class UpdateProfileCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfileCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserProfileDto> Handle(UpdateProfileCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user is null)
            throw new NotFoundException("User", command.UserId);

        if (command.DisplayName is not null)
            user.DisplayName = command.DisplayName;
        if (command.CalorieTarget is not null)
            user.CalorieTarget = command.CalorieTarget.Value;
        if (command.GoalType is not null)
            user.GoalType = command.GoalType.Value;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return new UserProfileDto(
            user.Id,
            user.Email,
            user.DisplayName,
            user.CalorieTarget,
            user.GoalType,
            user.CreatedAt
        );
    }
}
