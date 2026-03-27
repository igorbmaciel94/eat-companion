namespace EatCompanion.Application.DTOs;

public record AuthResponseDto(string AccessToken, string RefreshToken, UserProfileDto User);
