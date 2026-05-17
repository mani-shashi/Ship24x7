using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ship24X7.Tracking.Application.Commands;
using Ship24X7.Tracking.Application.Queries;

namespace Ship24X7.Tracking.API.Controllers;

/// <summary>
/// API controller for managing delivery proof operations in the Ship24X7 tracking service.
/// Handles HTTP requests for capturing and retrieving delivery proof documentation.
/// Delivery proof includes recipient signature, photo evidence, GPS coordinates, and delivery timestamp.
/// Critical for confirming successful delivery and resolving delivery disputes.
/// Only accessible when shipment is in OutForDelivery status.
/// Workflow: Shipment out for delivery -> Delivery personnel captures proof -> Shipment marked delivered -> Proof available for query.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class DeliveryProofController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DeliveryProofController> _logger;

    /// <summary>
    /// Initializes a new instance of the DeliveryProofController class.
    /// Sets up dependencies for command/query processing and logging.
    /// </summary>
    /// <param name="mediator">MediatR instance for sending commands and queries to their respective handlers</param>
    /// <param name="logger">Logger instance for recording delivery proof operations, errors, and diagnostic information</param>
    public DeliveryProofController(IMediator mediator, ILogger<DeliveryProofController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Captures delivery proof when shipment is delivered to recipient.
    /// Endpoint: POST /api/v1/deliveryproof
    /// Logic flow:
    /// 1. Receives delivery proof data including signature image (base64), photo proof (base64), GPS coordinates, and recipient name
    /// 2. Validates GPS coordinates are within valid ranges (latitude: -90 to 90, longitude: -180 to 180)
    /// 3. Checks shipment is in OutForDelivery status (only deliverable shipments can have proof captured)
    /// 4. If status invalid, returns 409 Conflict with error message
    /// 5. Converts base64 signature and photo to byte arrays
    /// 6. Uploads signature image to blob storage with filename format: {trackingNumber}_signature_{timestamp}.png
    /// 7. Uploads photo proof to blob storage with filename format: {trackingNumber}_photo_{timestamp}.jpg
    /// 8. Creates DeliveryProof entity with recipient name, delivery date, image URLs, GPS coordinates, and notes
    /// 9. Saves delivery proof to database
    /// 10. Records tracking event with status Delivered and location as GPS coordinates
    /// 11. Publishes ShipmentDelivered domain event for downstream services (notification, shipment service)
    /// 12. Returns delivery proof details with 201 Created status
    /// Used by delivery personnel mobile app to capture proof at delivery time.
    /// </summary>
    /// <param name="command">Command containing shipment ID, tracking number, recipient name, delivery date, signature/photo (base64), GPS coordinates, notes, and delivery personnel ID</param>
    /// <returns>
    /// 201 Created with DeliveryProofResponse containing proof ID, shipment ID, recipient name, delivery date, image URLs, and GPS coordinates.
    /// 400 Bad Request if validation fails (invalid GPS coordinates, missing required fields).
    /// 409 Conflict if shipment is not in OutForDelivery status.
    /// 422 Unprocessable Entity if image data is invalid or cannot be processed.
    /// 500 Internal Server Error for unexpected errors during proof capture.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> CaptureDeliveryProof([FromBody] CaptureDeliveryProofCommand command)
    {
        try
        {
            var response = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetDeliveryProof), new { shipmentId = response.ShipmentId }, response);
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message.Contains("OutForDelivery") 
                ? Conflict(new { error = ex.Message }) 
                : BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return UnprocessableEntity(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing delivery proof");
            return StatusCode(500, new { error = "An error occurred while capturing delivery proof" });
        }
    }

    /// <summary>
    /// Retrieves delivery proof for a shipment by shipment ID.
    /// Endpoint: GET /api/v1/deliveryproof/{shipmentId}
    /// Logic flow:
    /// 1. Receives shipment ID from route parameter and optional customer ID from query string
    /// 2. Creates query with shipment ID and customer ID
    /// 3. Sends query to handler via MediatR to fetch delivery proof from database
    /// 4. If customer ID provided, validates customer owns the shipment (authorization check)
    /// 5. If customer does not own shipment, returns 403 Forbidden
    /// 6. If delivery proof not found, returns 404 Not Found
    /// 7. Returns delivery proof with recipient name, delivery date, signature/photo URLs, GPS coordinates, and notes
    /// Used by customers, customer service, and admin dashboards to view delivery confirmation.
    /// Useful for resolving delivery disputes and confirming successful delivery.
    /// </summary>
    /// <param name="shipmentId">Unique identifier of the shipment to retrieve delivery proof for</param>
    /// <param name="customerId">Optional customer ID for authorization check (ensures customer owns the shipment)</param>
    /// <returns>
    /// 200 OK with DeliveryProofResponse containing proof details, recipient name, delivery date, image URLs, and GPS coordinates.
    /// 403 Forbidden if customer ID provided but customer does not own the shipment.
    /// 404 Not Found if delivery proof does not exist for the shipment.
    /// 500 Internal Server Error for unexpected errors during retrieval.
    /// </returns>
    [HttpGet("{shipmentId}")]
    public async Task<IActionResult> GetDeliveryProof(Guid shipmentId, [FromQuery] Guid? customerId = null)
    {
        try
        {
            var query = new GetDeliveryProofQuery { ShipmentId = shipmentId, CustomerId = customerId };
            var response = await _mediator.Send(query);
            
            if (response == null)
                return NotFound(new { error = "Delivery proof not found" });
            
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving delivery proof");
            return StatusCode(500, new { error = "An error occurred while retrieving delivery proof" });
        }
    }
}
