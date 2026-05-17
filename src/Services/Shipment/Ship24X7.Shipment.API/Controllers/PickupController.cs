using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ship24X7.Shipment.Application.Commands;
using Ship24X7.Shipment.Application.Queries;

namespace Ship24X7.Shipment.API.Controllers;

/// <summary>
/// Manages pickup scheduling, cancellation, and completion.
/// All exception handling is delegated to GlobalExceptionMiddleware.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class PickupController : ControllerBase
{
    private readonly IMediator _mediator;

    public PickupController(IMediator mediator) => _mediator = mediator;

    // POST /api/v1/Pickup
    [HttpPost]
    public async Task<IActionResult> SchedulePickup([FromBody] SchedulePickupCommand command)
    {
        var pickupId = await _mediator.Send(command);
        return CreatedAtAction(nameof(SchedulePickup), new { id = pickupId }, new { pickupId });
    }

    // PUT /api/v1/Pickup/{id}/cancel
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelPickup(Guid id, [FromBody] CancelPickupCommand command)
    {
        command.PickupId = id;
        var result = await _mediator.Send(command);
        return Ok(new { success = result, message = "Pickup cancelled successfully" });
    }

    // PUT /api/v1/Pickup/{id}/complete
    [HttpPut("{id}/complete")]
    public async Task<IActionResult> CompletePickup(Guid id, [FromBody] CompletePickupCommand command)
    {
        command.PickupId = id;
        var result = await _mediator.Send(command);
        return Ok(new { success = result, message = "Pickup completed successfully" });
    }

    // GET /api/v1/Pickup/slots?date=...
    [HttpGet("slots")]
    public async Task<IActionResult> GetAvailablePickupSlots([FromQuery] DateTime date)
        => Ok(await _mediator.Send(new GetAvailablePickupSlotsQuery { Date = date }));
}
