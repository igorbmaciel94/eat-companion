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
    private readonly UpdateWeightEntryCommandHandler _updateHandler;
    private readonly IValidator<AddWeightEntryCommand> _addValidator;

    public WeightEntriesController(
        AddWeightEntryCommandHandler addHandler,
        GetWeightEntriesQueryHandler getHandler,
        UpdateWeightEntryCommandHandler updateHandler,
        IValidator<AddWeightEntryCommand> addValidator)
    {
        _addHandler = addHandler;
        _getHandler = getHandler;
        _updateHandler = updateHandler;
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

    public record UpdateWeightEntryRequest(decimal WeightKg, string? Notes);

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWeightEntryRequest request)
    {
        var command = new UpdateWeightEntryCommand(GetUserId(), id, request.WeightKg, request.Notes);
        var result = await _updateHandler.Handle(command);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
    {
        var result = await _getHandler.Handle(new GetWeightEntriesQuery(GetUserId(), startDate, endDate));
        return Ok(result);
    }
}
