using EatCompanion.Application.DTOs;
using EatCompanion.Application.Interfaces;
using EatCompanion.Application.Common;
using EatCompanion.Domain.Interfaces;

namespace EatCompanion.Application.UseCases.Auth;

public record RefreshTokenCommand(string RefreshToken);

public class RefreshTokenCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand command)
    {
        var (user, existingToken) = await _userRepository.GetByRefreshTokenAsync(command.RefreshToken);

        if (user is null || existingToken is null || existingToken.IsRevoked || existingToken.ExpiresAt < DateTime.UtcNow)
            throw new Common.ValidationException("RefreshToken", "Invalid or expired refresh token.");

        existingToken.IsRevoked = true;

        var newRefreshToken = _tokenService.GenerateRefreshToken(user.Id);
        user.RefreshTokens.Add(newRefreshToken);
        await _unitOfWork.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(user);

        return new AuthResponseDto(
            accessToken,
            newRefreshToken.Token,
            newRefreshToken.ExpiresAt,
            new UserProfileDto(user.Id, user.Email, user.DisplayName, user.CalorieTarget, user.GoalType, user.CreatedAt)
        );
    }
}
