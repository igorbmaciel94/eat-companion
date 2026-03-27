using System.Security.Claims;
using EatCompanion.Application.UseCases.WeightEntries;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatCompanion.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/weight-entries")]
public class WeightEntriesController : ControllerBase
{
    private readonly AddWeightEntryCommandHandler _addHandler;
    private readonly GetWeightEntriesQueryHandler _getHandler;
    private readonly IValidator<AddWeightEntryCommand> _addValidator;

    public WeightEntriesController(
        AddWeightEntryCommandHandler addHandler,
        GetWeightEntriesQueryHandler getHandler,
        IValidator<AddWeightEntryCommand> addValidator)
    {
        _addHandler = addHandler;
        _getHandler = getHandler;
        _addValidator = addValidator;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

    public record AddWeightEntryRequest(DateOnly Date, decimal WeightKg, string? Notes);

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddWeightEntryRequest request)
    {
        var command = new AddWeightEntryCommand(GetUserId(), request.Date, request.WeightKg, request.Notes);

        var validation = await _addValidator.ValidateAsync(command);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new Application.Common.ValidationException(errors);
        }

        var result = await _addHandler.Handle(command);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
    {
        var result = await _getHandler.Handle(new GetWeightEntriesQuery(GetUserId(), startDate, endDate));
        return Ok(result);
    }
}
