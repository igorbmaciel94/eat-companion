using System.Security.Claims;
using EatCompanion.Application.UseCases.MealPlans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatCompanion.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/meal-plans")]
public class MealPlansController : ControllerBase
{
    private readonly GetMealPlansQueryHandler _getMealPlansHandler;
    private readonly GetMealPlanQueryHandler _getMealPlanHandler;
    private readonly GetDailySummaryQueryHandler _getDailySummaryHandler;
    private readonly SelectMealOptionCommandHandler _selectOptionHandler;
    private readonly ImportMealPlanCommandHandler _importHandler;

    public MealPlansController(
        GetMealPlansQueryHandler getMealPlansHandler,
        GetMealPlanQueryHandler getMealPlanHandler,
        GetDailySummaryQueryHandler getDailySummaryHandler,
        SelectMealOptionCommandHandler selectOptionHandler,
        ImportMealPlanCommandHandler importHandler)
    {
        _getMealPlansHandler = getMealPlansHandler;
        _getMealPlanHandler = getMealPlanHandler;
        _getDailySummaryHandler = getDailySummaryHandler;
        _selectOptionHandler = selectOptionHandler;
        _importHandler = importHandler;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

    [HttpPost("import")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest("A PDF file is required.");

        using var stream = file.OpenReadStream();
        var result = await _importHandler.Handle(new ImportMealPlanCommand(stream, file.FileName, GetUserId()));
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

    [HttpPut("{id:guid}/meals/{mealId:guid}/options/{optionId:guid}/select")]
    public async Task<IActionResult> SelectOption(Guid id, Guid mealId, Guid optionId)
    {
        await _selectOptionHandler.Handle(new SelectMealOptionCommand(id, mealId, optionId, GetUserId()));
        return NoContent();
    }
}
