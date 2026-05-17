using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ship24X7.Shipment.Application.Queries;

namespace Ship24X7.Shipment.API.Controllers;

/// <summary>
/// API controller for managing dashboard and reporting operations in the Ship24X7 shipment service.
/// Handles HTTP requests for retrieving dashboard summaries and exception reports.
/// Provides aggregated statistics and insights for admin and hub operator dashboards.
/// Supports real-time monitoring of shipment operations and exception tracking.
/// Workflow: Dashboard loads -> Fetch summary -> Display metrics -> Monitor exceptions -> Take action.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DashboardController> _logger;

    /// <summary>
    /// Initializes a new instance of the DashboardController class.
    /// Sets up dependencies for query processing and logging.
    /// </summary>
    /// <param name="mediator">MediatR instance for sending queries to their respective handlers</param>
    /// <param name="logger">Logger instance for recording dashboard operations, errors, and diagnostic information</param>
    public DashboardController(IMediator mediator, ILogger<DashboardController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves dashboard summary with aggregated shipment statistics.
    /// Endpoint: GET /api/v1/dashboard/summary
    /// Logic flow:
    /// 1. Creates query for dashboard summary
    /// 2. Sends query to handler via MediatR for processing
    /// 3. Handler counts shipments by status:
    ///    - Total shipments (all statuses)
    ///    - Draft shipments (awaiting payment)
    ///    - Confirmed shipments (payment completed, awaiting pickup)
    ///    - PickedUp shipments (collected from customer)
    ///    - InTransit shipments (on the way to destination)
    ///    - OutForDelivery shipments (out for final delivery)
    ///    - Delivered shipments (successfully delivered)
    ///    - Cancelled shipments (cancelled by customer or system)
    /// 4. Handler counts pending pickups (scheduled but not completed)
    /// 5. Handler counts shipments with exceptions (delays, failed delivery attempts)
    /// 6. Handler calculates today's revenue from delivered shipments
    /// 7. Handler calculates average delivery time for completed shipments
    /// 8. Returns dashboard summary with all metrics
    /// Used by admin and hub operator dashboards to monitor operations.
    /// Refreshed periodically (e.g., every 30 seconds) for real-time monitoring.
    /// </summary>
    /// <returns>
    /// 200 OK with DashboardSummaryResponse containing shipment counts by status, pending pickups, exceptions, revenue, and average delivery time.
    /// 500 Internal Server Error for unexpected errors during summary retrieval.
    /// </returns>
    [HttpGet("summary")]
    public async Task<IActionResult> GetDashboardSummary()
    {
        try
        {
            var query = new GetDashboardSummaryQuery();
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard summary");
            return StatusCode(500, new { error = "An error occurred while retrieving dashboard summary" });
        }
    }

    /// <summary>
    /// Retrieves exceptions report with list of shipments requiring attention.
    /// Endpoint: GET /api/v1/dashboard/exceptions
    /// Logic flow:
    /// 1. Creates query for exceptions report
    /// 2. Sends query to handler via MediatR for processing
    /// 3. Handler retrieves shipments with tracking events marked as exceptions
    /// 4. Handler groups exceptions by type:
    ///    - Delayed shipments (in transit longer than expected)
    ///    - Failed delivery attempts (customer not available)
    ///    - Address issues (incorrect or incomplete address)
    ///    - Damaged packages (reported during transit)
    ///    - Lost shipments (no tracking updates for extended period)
    /// 5. Handler includes shipment details: tracking number, status, exception reason, location, timestamp
    /// 6. Handler orders by exception timestamp (most recent first)
    /// 7. Returns list of exceptions for review and action
    /// Used by customer service and hub operators to identify and resolve issues.
    /// Helps prioritize shipments requiring immediate attention.
    /// </summary>
    /// <returns>
    /// 200 OK with list of exception records containing shipment details, exception type, reason, location, and timestamp.
    /// 500 Internal Server Error for unexpected errors during report retrieval.
    /// </returns>
    [HttpGet("exceptions")]
    public async Task<IActionResult> GetExceptionsReport()
    {
        try
        {
            var query = new GetExceptionsReportQuery();
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving exceptions report");
            return StatusCode(500, new { error = "An error occurred while retrieving exceptions report" });
        }
    }
}
