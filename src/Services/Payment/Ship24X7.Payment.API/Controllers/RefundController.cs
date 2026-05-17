using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ship24X7.Payment.Application.Commands;
using Ship24X7.Payment.Application.Queries;

namespace Ship24X7.Payment.API.Controllers;

/// <summary>
/// API controller for managing refund operations in the Ship24X7 payment service.
/// Handles HTTP requests for initiating refunds and retrieving refund status.
/// This controller integrates with Razorpay payment gateway for processing refunds.
/// Access restricted to Admin_User and System_Admin roles only for security and compliance.
/// Workflow: Admin initiates refund -> Razorpay processes refund -> Webhook updates status -> Refund completed.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin_User,System_Admin")]
public class RefundController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RefundController> _logger;

    /// <summary>
    /// Initializes a new instance of the RefundController class.
    /// Sets up dependencies for command/query processing and logging.
    /// </summary>
    /// <param name="mediator">MediatR instance for sending commands and queries to their respective handlers</param>
    /// <param name="logger">Logger instance for recording refund operations, errors, and diagnostic information</param>
    public RefundController(IMediator mediator, ILogger<RefundController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Initiates a refund for a captured payment order.
    /// Endpoint: POST /api/v1/refund
    /// Logic flow:
    /// 1. Receives refund request with payment order ID, amount, reason, and admin user ID
    /// 2. Validates that payment order exists and is in Captured status
    /// 3. Validates refund amount is positive and does not exceed original payment amount
    /// 4. Calls Razorpay API to initiate refund with payment ID and amount
    /// 5. Creates PaymentRefund entity with Initiated status and stores in database
    /// 6. Returns refund details including refund ID and Razorpay refund ID
    /// 7. Webhook will later update status to Processed when Razorpay completes refund
    /// Only accessible by Admin_User and System_Admin roles for security.
    /// </summary>
    /// <param name="command">Command containing payment order ID, refund amount, reason, and admin user ID who initiated the refund</param>
    /// <returns>
    /// 202 Accepted with RefundResponse containing refund ID, status, amount, currency, and Razorpay refund ID.
    /// 400 Bad Request if payment order not found, invalid status, or invalid refund amount.
    /// 500 Internal Server Error for unexpected errors or Razorpay service failures.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> InitiateRefund([FromBody] InitiateRefundCommand command)
    {
        try
        {
            var response = await _mediator.Send(command);
            return Accepted(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error initiating refund");
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error initiating refund");
            return StatusCode(500, new { error = "An error occurred while initiating refund" });
        }
    }

    /// <summary>
    /// Retrieves refund status and details by refund ID.
    /// Endpoint: GET /api/v1/refund/{refundId}
    /// Logic flow:
    /// 1. Receives refund ID from route parameter
    /// 2. Creates query with refund ID
    /// 3. Sends query to handler via MediatR to fetch refund from database
    /// 4. Returns refund details if found, otherwise returns 404 Not Found
    /// Used by admin dashboards to track refund status (Initiated, Processed, Failed).
    /// Only accessible by Admin_User and System_Admin roles.
    /// </summary>
    /// <param name="refundId">Unique identifier of the refund to retrieve</param>
    /// <returns>
    /// 200 OK with refund details including status, amount, Razorpay refund ID, reason, timestamps, and initiator.
    /// 404 Not Found if refund with specified ID does not exist.
    /// 500 Internal Server Error for unexpected errors during retrieval.
    /// </returns>
    [HttpGet("{refundId}")]
    public async Task<IActionResult> GetRefundStatus(Guid refundId)
    {
        try
        {
            var query = new GetRefundStatusQuery { RefundId = refundId };
            var refund = await _mediator.Send(query);

            if (refund == null)
            {
                return NotFound(new { error = "Refund not found" });
            }

            return Ok(refund);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving refund status");
            return StatusCode(500, new { error = "An error occurred while retrieving refund status" });
        }
    }
}
