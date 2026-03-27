using System.Security.Claims;
using EatCompanion.Application.UseCases.Profile;
using EatCompanion.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatCompanion.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/profile")]
public class ProfileController : ControllerBase
{
    private readonly GetProfileQueryHandler _getHandler;
    private readonly UpdateProfileCommandHandler _updateHandler;
    private readonly IValidator<UpdateProfileCommand> _updateValidator;

    public ProfileController(
        GetProfileQueryHandler getHandler,
        UpdateProfileCommandHandler updateHandler,
        IValidator<UpdateProfileCommand> updateValidator)
    {
        _getHandler = getHandler;
        _updateHandler = updateHandler;
        _updateValidator = updateValidator;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _getHandler.Handle(new GetProfileQuery(GetUserId()));
        return Ok(result);
    }

    public record UpdateProfileRequest(string DisplayName, int CalorieTarget, GoalType GoalType);

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateProfileRequest request)
    {
        var command = new UpdateProfileCommand(GetUserId(), request.DisplayName, request.CalorieTarget, request.GoalType);

        var validation = await _updateValidator.ValidateAsync(command);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new Application.Common.ValidationException(errors);
        }

        var result = await _updateHandler.Handle(command);
        return Ok(result);
    }
}
