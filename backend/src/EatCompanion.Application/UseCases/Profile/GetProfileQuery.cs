using EatCompanion.Application.Common;
using EatCompanion.Application.DTOs;
using EatCompanion.Domain.Interfaces;

namespace EatCompanion.Application.UseCases.Profile;

public record GetProfileQuery(Guid UserId);

public class GetProfileQueryHandler
{
    private readonly IUserRepository _userRepository;

    public GetProfileQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileDto> Handle(GetProfileQuery query)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId);
        if (user is null)
            throw new NotFoundException("User", query.UserId);

        return new UserProfileDto(
            user.Id,
            user.Email,
            user.DisplayName,
            user.CalorieTarget,
            user.GoalType,
            user.HeightCm,
            user.WeightKg,
            user.CreatedAt
        );
    }
}
