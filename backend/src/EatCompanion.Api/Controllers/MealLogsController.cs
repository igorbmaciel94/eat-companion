using System.Security.Claims;
using EatCompanion.Application.UseCases.MealLogs;
using EatCompanion.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatCompanion.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/meal-logs")]
public class MealLogsController : ControllerBase
{
    private readonly LogMealCommandHandler _logMealHandler;
    private readonly GetMealLogsQueryHandler _getMealLogsHandler;
    private readonly IValidator<LogMealCommand> _logMealValidator;

    public MealLogsController(
        LogMealCommandHandler logMealHandler,
        GetMealLogsQueryHandler getMealLogsHandler,
        IValidator<LogMealCommand> logMealValidator)
    {
        _logMealHandler = logMealHandler;
        _getMealLogsHandler = getMealLogsHandler;
        _logMealValidator = logMealValidator;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

    public record LogMealRequest(DateOnly Date, MealType MealType, MealLogStatus Status, Guid? MealOptionId, string? Notes);

    [HttpPost]
    public async Task<IActionResult> LogMeal([FromBody] LogMealRequest request)
    {
        var command = new LogMealCommand(GetUserId(), request.Date, request.MealType, request.Status, request.MealOptionId, request.Notes);

        var validation = await _logMealValidator.ValidateAsync(command);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new Application.Common.ValidationException(errors);
        }

        var result = await _logMealHandler.Handle(command);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetMealLogs([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
    {
        var result = await _getMealLogsHandler.Handle(new GetMealLogsQuery(GetUserId(), startDate, endDate));
        return Ok(result);
    }
}
