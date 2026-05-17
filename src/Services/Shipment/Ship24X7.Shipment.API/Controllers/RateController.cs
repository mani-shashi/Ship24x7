using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ship24X7.Shipment.Application.Commands;
using Ship24X7.Shipment.Application.Queries;

namespace Ship24X7.Shipment.API.Controllers;

/// <summary>
/// API controller for managing shipping rate calculation operations in the Ship24X7 shipment service.
/// Handles HTTP requests for calculating shipping rates based on package details and service type.
/// Rate calculation considers weight, dimensions, distance, service type, and declared value.
/// Provides instant rate quotes to customers before shipment creation.
/// Supports multiple service types: Standard (3-5 days), Express (1-2 days), Overnight (next day).
/// Workflow: Customer enters package details -> Calculate rate -> Display quote -> Customer creates shipment.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class RateController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RateController> _logger;

    /// <summary>
    /// Initializes a new instance of the RateController class.
    /// Sets up dependencies for query processing and logging.
    /// </summary>
    /// <param name="mediator">MediatR instance for sending queries to their respective handlers</param>
    /// <param name="logger">Logger instance for recording rate calculation operations, errors, and diagnostic information</param>
    public RateController(IMediator mediator, ILogger<RateController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Calculates shipping rate for a package based on weight, dimensions, service type, and locations.
    /// Endpoint: POST /api/v1/rate/calculate
    /// Logic flow:
    /// 1. Receives rate calculation request with origin/destination postal codes, package dimensions, weight, service type, and declared value
    /// 2. Sends query to handler via MediatR for processing
    /// 3. Handler validates postal codes exist in database
    /// 4. If postal codes invalid, returns 400 Bad Request
    /// 5. Handler calculates distance between origin and destination using postal code coordinates
    /// 6. Handler retrieves service rate configuration from database (base rate, per kg rate, per km rate)
    /// 7. Handler calculates volumetric weight: (Length × Width × Height) / 5000
    /// 8. Handler uses chargeable weight: max(actual weight, volumetric weight)
    /// 9. Handler calculates base shipping cost: base rate + (chargeable weight × per kg rate) + (distance × per km rate)
    /// 10. Handler applies service type multiplier: Standard (1.0x), Express (1.5x), Overnight (2.0x)
    /// 11. Handler calculates insurance cost: 1% of declared value (if declared value > 0)
    /// 12. Handler calculates taxes: 18% GST on (shipping cost + insurance cost)
    /// 13. Handler calculates total cost: shipping cost + insurance cost + taxes
    /// 14. Handler estimates delivery time based on service type and distance
    /// 15. Returns rate breakdown with shipping cost, insurance, taxes, total, and estimated delivery
    /// Used by frontend to display instant rate quotes before shipment creation.
    /// Helps customers choose appropriate service type based on cost and delivery time.
    /// <summary>
    /// Retrieves all available shipping service rates.
    /// Endpoint: GET /api/v1/rate
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetServiceRates()
    {
        try
        {
            var query = new GetServiceRatesQuery();
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service rates");
            return StatusCode(500, new { error = "An error occurred while retrieving service rates" });
        }
    }

    /// <summary>
    /// Calculates shipping rate for a package based on weight, dimensions, service type, and locations.
    /// Endpoint: POST /api/v1/rate/calculate
    /// </summary>
    /// <param name="query">Query containing origin/destination postal codes, package dimensions (length, width, height in cm), weight (in kg), service type, and declared value</param>
    /// <returns>
    /// 200 OK with RateCalculationResponse containing shipping cost, insurance cost, taxes, total cost, estimated delivery date, and rate breakdown.
    /// 400 Bad Request if postal codes invalid, dimensions/weight invalid, or service type not supported.
    /// 500 Internal Server Error for unexpected errors during rate calculation.
    /// </returns>
    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateRate([FromBody] CalculateRateQuery query)
    {
        try
        {
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating rates");
            return StatusCode(500, new { error = "An error occurred while calculating rates" });
        }
    }

    /// <summary>
    /// Creates a new shipping service rate configuration.
    /// Endpoint: POST /api/v1/rate
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "System_Admin,Admin_User")]
    public async Task<IActionResult> CreateRate([FromBody] CreateServiceRateCommand command)
    {
        try
        {
            var rateId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetServiceRates), new { id = rateId }, new { id = rateId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create shipment service rate configuration");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Updates an existing shipping service rate configuration.
    /// Endpoint: PUT /api/v1/rate/{id}
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "System_Admin,Admin_User")]
    public async Task<IActionResult> UpdateRate(Guid id, [FromBody] UpdateServiceRateCommand command)
    {
        try
        {
            command.RateId = id;
            await _mediator.Send(command);
            return Ok(new { message = "Service rate pricing configuration updated" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Service rate profile not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update rate configuration {RateId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Retires (soft-deletes) a shipping service rate configuration.
    /// Endpoint: DELETE /api/v1/rate/{id}
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "System_Admin,Admin_User")]
    public async Task<IActionResult> RetireRate(Guid id)
    {
        try
        {
            var command = new RetireServiceRateCommand { RateId = id };
            await _mediator.Send(command);
            return Ok(new { message = "Pricing configuration retired successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Service rate profile not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retire pricing profile {RateId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
