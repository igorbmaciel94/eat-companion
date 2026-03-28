using EatCompanion.Application.Common;
using EatCompanion.Application.DTOs;
using EatCompanion.Application.Interfaces;
using EatCompanion.Application.Interfaces;
using EatCompanion.Domain.Entities;

namespace EatCompanion.Application.UseCases.Auth;

public record RegisterCommand(string Email, string Password, string DisplayName);

public class RegisterCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand command)
    {
        var existingUser = await _userRepository.GetByEmailAsync(command.Email);
        if (existingUser is not null)
            throw new Common.ValidationException("Email", "Email is already registered.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.Password),
            DisplayName = command.DisplayName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);
        user.RefreshTokens.Add(refreshToken);

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
