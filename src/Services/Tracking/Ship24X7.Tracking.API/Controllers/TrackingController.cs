using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ship24X7.Tracking.Application.Commands;
using Ship24X7.Tracking.Application.Queries;

namespace Ship24X7.Tracking.API.Controllers;

/// <summary>
/// API controller for managing shipment tracking operations in the Ship24X7 tracking service.
/// Handles HTTP requests for recording tracking events, retrieving tracking history, and public tracking.
/// This controller provides real-time shipment status updates and location tracking.
/// Supports both authenticated tracking (with customer verification) and public tracking (no authentication).
/// Workflow: Tracking events recorded -> Status updated -> Customers notified -> History available for query.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class TrackingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TrackingController> _logger;

    /// <summary>
    /// Initializes a new instance of the TrackingController class.
    /// Sets up dependencies for command/query processing and logging.
    /// </summary>
    /// <param name="mediator">MediatR instance for sending commands and queries to their respective handlers</param>
    /// <param name="logger">Logger instance for recording tracking operations, errors, and diagnostic information</param>
    public TrackingController(IMediator mediator, ILogger<TrackingController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Records a new tracking event for a shipment.
    /// Endpoint: POST /api/v1/tracking/events
    /// Logic flow:
    /// 1. Receives tracking event data including shipment ID, tracking number, status, location, and timestamp
    /// 2. Sends command to handler via MediatR for processing
    /// 3. Handler creates TrackingEvent entity and saves to database
    /// 4. Handler publishes TrackingEventRecorded domain event for downstream services
    /// 5. If event is an exception (delay), publishes ShipmentDelayed event for customer notification
    /// 6. Returns tracking event details with 201 Created status
    /// 7. Location header points to GetTrackingHistory endpoint for the shipment
    /// Used by hub operators, delivery personnel, and automated tracking systems to update shipment status.
    /// </summary>
    /// <param name="command">Command containing tracking event details including status, location, timestamp, and exception information</param>
    /// <returns>
    /// 201 Created with TrackingEventResponse containing event ID, shipment ID, tracking number, status, location, and timestamp.
    /// 400 Bad Request if validation fails or business rules are violated.
    /// 500 Internal Server Error for unexpected errors during event recording.
    /// </returns>
    [HttpPost("events")]
    public async Task<IActionResult> RecordTrackingEvent([FromBody] RecordTrackingEventCommand command)
    {
        try
        {
            var response = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetTrackingHistory), new { trackingNumber = response.TrackingNumber }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording tracking event");
            return StatusCode(500, new { error = "An error occurred while recording the tracking event" });
        }
    }

    /// <summary>
    /// Retrieves complete tracking history for a shipment by tracking number.
    /// Endpoint: GET /api/v1/tracking/{trackingNumber}
    /// Logic flow:
    /// 1. Receives tracking number from route parameter and optional customer ID from query string
    /// 2. Creates query with tracking number and customer ID
    /// 3. Sends query to handler via MediatR to fetch all tracking events for the shipment
    /// 4. Handler retrieves events from database ordered by event timestamp (chronological order)
    /// 5. If customer ID provided, validates customer owns the shipment (authorization check)
    /// 6. Returns tracking history with all events, current status, and location information
    /// Used by customers, customer service, and admin dashboards to view shipment journey.
    /// </summary>
    /// <param name="trackingNumber">Tracking number of the shipment to retrieve history for</param>
    /// <param name="customerId">Optional customer ID for authorization check (ensures customer owns the shipment)</param>
    /// <returns>
    /// 200 OK with TrackingHistoryResponse containing list of tracking events with status, location, timestamp, and exception details.
    /// 404 Not Found if tracking number does not exist or customer does not own the shipment.
    /// 500 Internal Server Error for unexpected errors during retrieval.
    /// </returns>
    [HttpGet("{trackingNumber}")]
    public async Task<IActionResult> GetTrackingHistory(string trackingNumber, [FromQuery] Guid? customerId = null)
    {
        try
        {
            var query = new GetTrackingHistoryQuery { TrackingNumber = trackingNumber, CustomerId = customerId };
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tracking history");
            return StatusCode(500, new { error = "An error occurred while retrieving tracking history" });
        }
    }

    /// <summary>
    /// Retrieves public tracking information for a shipment (no authentication required).
    /// Endpoint: GET /api/v1/tracking/public/{trackingNumber}
    /// Logic flow:
    /// 1. Receives tracking number from route parameter
    /// 2. Creates query with tracking number
    /// 3. Sends query to handler via MediatR to fetch public tracking information
    /// 4. Handler retrieves latest tracking event and basic shipment information
    /// 5. Returns simplified tracking information without sensitive details
    /// 6. If tracking number not found, returns 404 Not Found
    /// Used by customers and recipients to track shipments without logging in.
    /// Provides limited information compared to authenticated tracking (no customer-specific details).
    /// </summary>
    /// <param name="trackingNumber">Tracking number of the shipment to retrieve public tracking for</param>
    /// <returns>
    /// 200 OK with PublicTrackingResponse containing current status, latest location, estimated delivery, and basic shipment info.
    /// 404 Not Found if tracking number does not exist.
    /// 500 Internal Server Error for unexpected errors during retrieval.
    /// </returns>
    [HttpGet("public/{trackingNumber}")]
    public async Task<IActionResult> GetPublicTracking(string trackingNumber)
    {
        try
        {
            var query = new GetPublicTrackingQuery { TrackingNumber = trackingNumber };
            var response = await _mediator.Send(query);
            
            if (response == null)
                return NotFound(new { error = "Tracking information not found" });
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving public tracking");
            return StatusCode(500, new { error = "An error occurred while retrieving tracking information" });
        }
    }
}
