using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ship24X7.Shipment.Application.Queries;

namespace Ship24X7.Shipment.API.Controllers;

/// <summary>
/// Exposes hub data for the admin panel and shipment wizard.
/// Route: GET /api/v1/Hub
/// Gateway maps: GET /api/v1/shipment/hubs → GET /api/v1/Hub (Port 9002)
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class HubController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<HubController> _logger;

    public HubController(IMediator mediator, ILogger<HubController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Returns all hubs.
    /// Query param: activeOnly=false to include deactivated hubs (admin use).
    /// GET /api/v1/Hub
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHubs([FromQuery] bool activeOnly = true)
    {
        try
        {
            var result = await _mediator.Send(new GetHubsQuery { ActiveOnly = activeOnly });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hubs");
            return StatusCode(500, new { error = "An error occurred while retrieving hubs" });
        }
    }
}
