using EatCompanion.Application.UseCases.Auth;
using EatCompanion.Application.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace EatCompanion.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly RegisterCommandHandler _registerHandler;
    private readonly LoginCommandHandler _loginHandler;
    private readonly RefreshTokenCommandHandler _refreshHandler;
    private readonly IValidator<RegisterCommand> _registerValidator;
    private readonly IValidator<LoginCommand> _loginValidator;

    public AuthController(
        RegisterCommandHandler registerHandler,
        LoginCommandHandler loginHandler,
        RefreshTokenCommandHandler refreshHandler,
        IValidator<RegisterCommand> registerValidator,
        IValidator<LoginCommand> loginValidator)
    {
        _registerHandler = registerHandler;
        _loginHandler = loginHandler;
        _refreshHandler = refreshHandler;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var validation = await _registerValidator.ValidateAsync(command);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new Application.Common.ValidationException(errors);
        }

        var result = await _registerHandler.Handle(command);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var validation = await _loginValidator.ValidateAsync(command);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new Application.Common.ValidationException(errors);
        }

        var result = await _loginHandler.Handle(command);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
    {
        var result = await _refreshHandler.Handle(command);
        return Ok(result);
    }
}
