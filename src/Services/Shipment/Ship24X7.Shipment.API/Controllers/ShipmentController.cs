using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ship24X7.Shipment.Application.Commands;
using Ship24X7.Shipment.Application.Queries;

namespace Ship24X7.Shipment.API.Controllers;

/// <summary>
/// Manages the full shipment lifecycle: create → confirm → pickup → transit → deliver.
/// All exception handling is delegated to GlobalExceptionMiddleware.
/// Controllers only express the happy path and domain-specific HTTP semantics (404, 403).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class ShipmentController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShipmentController(IMediator mediator) => _mediator = mediator;

    // POST /api/v1/Shipment
    [HttpPost]
    public async Task<IActionResult> CreateShipment([FromBody] CreateShipmentCommand command)
    {
        var response = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetShipmentById), new { id = response.Id }, response);
    }

    // GET /api/v1/Shipment/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetShipmentById(Guid id, [FromQuery] Guid? customerId = null)
    {
        var response = await _mediator.Send(new GetShipmentByIdQuery { ShipmentId = id, CustomerId = customerId });
        return response is null ? NotFound(new { error = "Shipment not found" }) : Ok(response);
    }

    // GET /api/v1/Shipment/tracking/{trackingNumber}
    [HttpGet("tracking/{trackingNumber}")]
    public async Task<IActionResult> GetShipmentByTrackingNumber(
        string trackingNumber, [FromQuery] Guid? customerId = null)
    {
        var response = await _mediator.Send(
            new GetShipmentByTrackingNumberQuery { TrackingNumber = trackingNumber, CustomerId = customerId });
        return response is null ? NotFound(new { error = "Shipment not found" }) : Ok(response);
    }

    // GET /api/v1/Shipment
    [HttpGet]
    public async Task<IActionResult> GetShipmentList([FromQuery] GetShipmentListQuery query)
        => Ok(await _mediator.Send(query));

    // PUT /api/v1/Shipment/{id}/confirm
    [HttpPut("{id}/confirm")]
    public async Task<IActionResult> ConfirmShipment(Guid id, [FromBody] ConfirmShipmentCommand command)
    {
        command.ShipmentId = id;
        return Ok(await _mediator.Send(command));
    }

    // PUT /api/v1/Shipment/{id}/cancel
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelShipment(Guid id, [FromBody] CancelShipmentCommand command)
    {
        command.ShipmentId = id;
        var result = await _mediator.Send(command);
        return Ok(new { success = result, message = "Shipment cancelled successfully" });
    }

    // PUT /api/v1/Shipment/{id}/status  (admin / event-driven override)
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateShipmentStatus(Guid id, [FromBody] UpdateShipmentStatusCommand command)
    {
        command.ShipmentId = id;
        var result = await _mediator.Send(command);
        return Ok(new { success = result, message = "Shipment status updated successfully" });
    }

    // PUT /api/v1/Shipment/{id}/hub
    [HttpPut("{id}/hub")]
    public async Task<IActionResult> AssignHub(Guid id, [FromBody] AssignHubCommand command)
    {
        command.ShipmentId = id;
        var result = await _mediator.Send(command);
        return result.Success
            ? Ok(new { success = true, hubName = result.HubName, message = result.Message })
            : BadRequest(new { error = result.Message });
    }

    // PUT /api/v1/Shipment/{id}/transit
    [HttpPut("{id}/transit")]
    public async Task<IActionResult> InitiateTransit(Guid id, [FromBody] InitiateTransitCommand command)
    {
        command.ShipmentId = id;
        var result = await _mediator.Send(command);
        return Ok(new { success = result, message = "Shipment is now in transit" });
    }

    // PUT /api/v1/Shipment/{id}/out-for-delivery
    [HttpPut("{id}/out-for-delivery")]
    public async Task<IActionResult> MarkOutForDelivery(Guid id, [FromBody] MarkOutForDeliveryCommand command)
    {
        command.ShipmentId = id;
        var result = await _mediator.Send(command);
        return result.Success
            ? Ok(new { success = true, message = result.Message })
            : BadRequest(new { error = result.Message });
    }

    // PUT /api/v1/Shipment/{id}/deliver
    [HttpPut("{id}/deliver")]
    public async Task<IActionResult> CompleteDelivery(Guid id, [FromBody] CompleteDeliveryCommand command)
    {
        if (command.SupervisorOverride)
        {
            var isPrivileged = User.IsInRole("Hub_User")
                            || User.IsInRole("Admin_User")
                            || User.IsInRole("System_Admin");
            if (!isPrivileged) return Forbid();
        }

        command.ShipmentId = id;
        var result = await _mediator.Send(command);
        return Ok(new { success = result, message = "Shipment delivered successfully" });
    }

    // GET /api/v1/Shipment/{id}/history
    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetStatusHistory(Guid id)
        => Ok(await _mediator.Send(new GetShipmentStatusHistoryQuery { ShipmentId = id }));
}
