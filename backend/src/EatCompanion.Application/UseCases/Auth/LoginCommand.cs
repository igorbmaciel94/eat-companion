using EatCompanion.Application.Common;
using EatCompanion.Application.DTOs;
using EatCompanion.Application.Interfaces;
using EatCompanion.Application.Interfaces;

namespace EatCompanion.Application.UseCases.Auth;

public record LoginCommand(string Email, string Password);

public class LoginCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand command)
    {
        var user = await _userRepository.GetByEmailAsync(command.Email.ToLowerInvariant());
        if (user is null || !BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
            throw new Common.UnauthorizedException();

        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);
        await _userRepository.AddRefreshTokenAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(user);

        return new AuthResponseDto(
            accessToken,
            refreshToken.Token,
            refreshToken.ExpiresAt,
            new UserProfileDto(user.Id, user.Email, user.DisplayName, user.CalorieTarget, user.GoalType, user.HeightCm, user.WeightKg, user.OnboardingCompleted, user.CreatedAt)
        );
    }
}
