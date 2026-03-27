using System.Security.Claims;
using EatCompanion.Application.UseCases.GroceryLists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatCompanion.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/grocery-lists")]
public class GroceryListsController : ControllerBase
{
    private readonly GetGroceryListQueryHandler _getGroceryListHandler;
    private readonly GetLatestGroceryListQueryHandler _getLatestHandler;
    private readonly ToggleGroceryItemCommandHandler _toggleItemHandler;
    private readonly GenerateGroceryListCommandHandler _generateHandler;

    public GroceryListsController(
        GetGroceryListQueryHandler getGroceryListHandler,
        GetLatestGroceryListQueryHandler getLatestHandler,
        ToggleGroceryItemCommandHandler toggleItemHandler,
        GenerateGroceryListCommandHandler generateHandler)
    {
        _getGroceryListHandler = getGroceryListHandler;
        _getLatestHandler = getLatestHandler;
        _toggleItemHandler = toggleItemHandler;
        _generateHandler = generateHandler;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

    public record GenerateRequest(Guid MealPlanId, DateOnly StartDate, DateOnly EndDate);

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateRequest request)
    {
        var result = await _generateHandler.Handle(new GenerateGroceryListCommand(request.MealPlanId, request.StartDate, request.EndDate, GetUserId()));
        return Ok(result);
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest([FromQuery] Guid mealPlanId)
    {
        var result = await _getLatestHandler.Handle(new GetLatestGroceryListQuery(mealPlanId));
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _getGroceryListHandler.Handle(new GetGroceryListQuery(id));
        return Ok(result);
    }

    [HttpPut("{id:guid}/items/{itemId:guid}/toggle")]
    public async Task<IActionResult> ToggleItem(Guid id, Guid itemId)
    {
        await _toggleItemHandler.Handle(new ToggleGroceryItemCommand(id, itemId));
        return NoContent();
    }
}
