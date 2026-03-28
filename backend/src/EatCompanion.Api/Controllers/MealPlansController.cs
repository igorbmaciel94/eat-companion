using System.Security.Claims;
using EatCompanion.Application.UseCases.MealPlans;
using EatCompanion.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatCompanion.Api.Controllers;

public record UpdateOptionRequest(string? Name, string? Description, int? Calories, decimal? ProteinGrams, decimal? CarbsGrams, decimal? FatGrams);
public record AddIngredientRequest(string Name, string? NamePt, decimal Amount, UnitOfMeasure Unit, IngredientCategory Category);
public record UpdateIngredientRequest(string Name, string? NamePt, decimal Amount, UnitOfMeasure Unit, IngredientCategory Category);

[ApiController]
[Authorize]
[Route("api/v1/meal-plans")]
public class MealPlansController : ControllerBase
{
    private readonly GetMealPlansQueryHandler _getMealPlansHandler;
    private readonly GetMealPlanQueryHandler _getMealPlanHandler;
    private readonly GetDailySummaryQueryHandler _getDailySummaryHandler;
    private readonly GetWeeklySummaryQueryHandler _getWeeklySummaryHandler;
    private readonly SelectMealOptionCommandHandler _selectOptionHandler;
    private readonly UpdateMealOptionCommandHandler _updateOptionHandler;
    private readonly AddIngredientCommandHandler _addIngredientHandler;
    private readonly UpdateIngredientCommandHandler _updateIngredientHandler;
    private readonly DeleteIngredientCommandHandler _deleteIngredientHandler;
    private readonly EnqueueImportCommandHandler _enqueueImportHandler;
    private readonly GetImportStatusQueryHandler _getImportStatusHandler;

    public MealPlansController(
        GetMealPlansQueryHandler getMealPlansHandler,
        GetMealPlanQueryHandler getMealPlanHandler,
        GetDailySummaryQueryHandler getDailySummaryHandler,
        GetWeeklySummaryQueryHandler getWeeklySummaryHandler,
        SelectMealOptionCommandHandler selectOptionHandler,
        UpdateMealOptionCommandHandler updateOptionHandler,
        AddIngredientCommandHandler addIngredientHandler,
        UpdateIngredientCommandHandler updateIngredientHandler,
        DeleteIngredientCommandHandler deleteIngredientHandler,
        EnqueueImportCommandHandler enqueueImportHandler,
        GetImportStatusQueryHandler getImportStatusHandler)
    {
        _getMealPlansHandler = getMealPlansHandler;
        _getMealPlanHandler = getMealPlanHandler;
        _getDailySummaryHandler = getDailySummaryHandler;
        _getWeeklySummaryHandler = getWeeklySummaryHandler;
        _selectOptionHandler = selectOptionHandler;
        _updateOptionHandler = updateOptionHandler;
        _addIngredientHandler = addIngredientHandler;
        _updateIngredientHandler = updateIngredientHandler;
        _deleteIngredientHandler = deleteIngredientHandler;
        _enqueueImportHandler = enqueueImportHandler;
        _getImportStatusHandler = getImportStatusHandler;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("A PDF file is required.");

        await using var stream = file.OpenReadStream();
        var jobId = await _enqueueImportHandler.Handle(
            new EnqueueImportCommand(stream, file.FileName, GetUserId()));

        return Accepted(new { jobId });
    }

    [HttpGet("import/{jobId:guid}/status")]
    public async Task<IActionResult> GetImportStatus(Guid jobId)
    {
        var result = await _getImportStatusHandler.Handle(
            new GetImportStatusQuery(jobId, GetUserId()));
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _getMealPlansHandler.Handle(new GetMealPlansQuery(GetUserId()));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _getMealPlanHandler.Handle(new GetMealPlanQuery(id, GetUserId()));
        return Ok(result);
    }

    [HttpGet("{id:guid}/days/{date}")]
    public async Task<IActionResult> GetDaySummary(Guid id, DateOnly date)
    {
        var result = await _getDailySummaryHandler.Handle(new GetDailySummaryQuery(id, date, GetUserId()));
        return Ok(result);
    }

    [HttpGet("{id:guid}/week")]
    public async Task<IActionResult> GetWeekSummary(Guid id)
    {
        var result = await _getWeeklySummaryHandler.Handle(new GetWeeklySummaryQuery(id, GetUserId()));
        return Ok(result);
    }

    [HttpPut("{id:guid}/meals/{mealId:guid}/options/{optionId:guid}/select")]
    public async Task<IActionResult> SelectOption(Guid id, Guid mealId, Guid optionId)
    {
        await _selectOptionHandler.Handle(new SelectMealOptionCommand(id, mealId, optionId, GetUserId()));
        return NoContent();
    }

    [HttpPut("{id:guid}/options/{optionId:guid}")]
    public async Task<IActionResult> UpdateOption(Guid id, Guid optionId, [FromBody] UpdateOptionRequest request)
    {
        await _updateOptionHandler.Handle(new UpdateMealOptionCommand(
            id, optionId, request.Name, request.Description,
            request.Calories, request.ProteinGrams, request.CarbsGrams, request.FatGrams,
            GetUserId()));
        return NoContent();
    }

    [HttpPost("{id:guid}/options/{optionId:guid}/ingredients")]
    public async Task<IActionResult> AddIngredient(Guid id, Guid optionId, [FromBody] AddIngredientRequest request)
    {
        await _addIngredientHandler.Handle(new AddIngredientCommand(
            id, optionId, request.Name, request.NamePt,
            request.Amount, request.Unit, request.Category,
            GetUserId()));
        return NoContent();
    }

    [HttpPut("{id:guid}/ingredients/{ingredientId:guid}")]
    public async Task<IActionResult> UpdateIngredient(Guid id, Guid ingredientId, [FromBody] UpdateIngredientRequest request)
    {
        await _updateIngredientHandler.Handle(new UpdateIngredientCommand(
            id, ingredientId, request.Name, request.NamePt,
            request.Amount, request.Unit, request.Category,
            GetUserId()));
        return NoContent();
    }

    [HttpDelete("{id:guid}/ingredients/{ingredientId:guid}")]
    public async Task<IActionResult> DeleteIngredient(Guid id, Guid ingredientId)
    {
        await _deleteIngredientHandler.Handle(new DeleteIngredientCommand(id, ingredientId, GetUserId()));
        return NoContent();
    }
}
