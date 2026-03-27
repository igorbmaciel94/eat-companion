using System.Security.Claims;
using EatCompanion.Application.UseCases.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EatCompanion.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly GetWeeklyAdherenceQueryHandler _adherenceHandler;
    private readonly GetWeightTrendQueryHandler _weightTrendHandler;

    public AnalyticsController(
        GetWeeklyAdherenceQueryHandler adherenceHandler,
        GetWeightTrendQueryHandler weightTrendHandler)
    {
        _adherenceHandler = adherenceHandler;
        _weightTrendHandler = weightTrendHandler;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

    [HttpGet("adherence")]
    public async Task<IActionResult> GetAdherence([FromQuery] int weeks = 4)
    {
        var result = await _adherenceHandler.Handle(new GetWeeklyAdherenceQuery(GetUserId(), weeks));
        return Ok(result);
    }

    [HttpGet("weight-trend")]
    public async Task<IActionResult> GetWeightTrend([FromQuery] int months = 3)
    {
        var result = await _weightTrendHandler.Handle(new GetWeightTrendQuery(GetUserId(), months));
        return Ok(result);
    }
}
