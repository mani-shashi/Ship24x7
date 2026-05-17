using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ship24X7.Payment.Application.Commands;

namespace Ship24X7.Payment.API.Controllers;

/// <summary>
/// API controller for handling Razorpay webhook notifications in the Ship24X7 payment service.
/// Processes asynchronous payment events from Razorpay including payment capture, failure, and refund completion.
/// This controller does NOT require authentication as webhooks come from Razorpay servers.
/// Security: Validates webhook signature using HMAC-SHA256 to ensure authenticity and prevent tampering.
/// Workflow: Razorpay sends webhook -> Signature validated -> Event processed -> Database updated -> Domain event published.
/// Critical for real-time payment status updates and ensuring data consistency between Razorpay and Ship24X7.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WebhookController> _logger;

    /// <summary>
    /// Initializes a new instance of the WebhookController class.
    /// Sets up dependencies for command processing and logging.
    /// </summary>
    /// <param name="mediator">MediatR instance for sending webhook processing commands to handler</param>
    /// <param name="logger">Logger instance for recording webhook events, signature validation, and errors</param>
    public WebhookController(IMediator mediator, ILogger<WebhookController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Handles incoming webhook notifications from Razorpay payment gateway.
    /// Endpoint: POST /api/v1/webhook/razorpay
    /// Logic flow:
    /// 1. Reads raw request body containing webhook payload (JSON)
    /// 2. Extracts X-Razorpay-Signature header for security validation
    /// 3. Returns 400 Bad Request if signature header is missing
    /// 4. Parses payload to extract event type (payment.captured, payment.failed, refund.processed)
    /// 5. Creates ProcessWebhookCommand with event type, payload, and signature
    /// 6. Sends command to handler which validates signature using HMAC-SHA256 with webhook secret
    /// 7. If signature invalid, returns 400 Bad Request and logs warning
    /// 8. If signature valid, handler processes event based on type:
    ///    - payment.captured: Updates payment order to Captured, publishes PaymentCaptured event
    ///    - payment.failed: Updates payment order to Failed, publishes PaymentFailed event
    ///    - refund.processed: Updates refund to Processed, updates payment order to Refunded, publishes RefundProcessed event
    /// 9. Returns 200 OK even on processing errors to prevent Razorpay from retrying
    /// Critical for maintaining payment state consistency and triggering downstream workflows.
    /// </summary>
    /// <returns>
    /// 200 OK if webhook signature is valid and event is processed successfully, or if processing fails (to prevent retries).
    /// 400 Bad Request if signature header is missing or signature validation fails.
    /// </returns>
    [HttpPost("razorpay")]
    public async Task<IActionResult> HandleRazorpayWebhook()
    {
        try
        {
            // Read raw body
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();

            // Get signature from header
            var signature = Request.Headers["X-Razorpay-Signature"].FirstOrDefault();
            if (string.IsNullOrEmpty(signature))
            {
                _logger.LogWarning("Webhook received without signature");
                return BadRequest(new { error = "Missing signature" });
            }

            // Get event type from payload
            var eventType = System.Text.Json.JsonDocument.Parse(payload)
                .RootElement
                .GetProperty("event")
                .GetString() ?? string.Empty;

            var command = new ProcessWebhookCommand
            {
                EventType = eventType,
                Payload = payload,
                Signature = signature
            };

            var isValid = await _mediator.Send(command);

            if (!isValid)
            {
                _logger.LogWarning("Invalid webhook signature");
                return BadRequest(new { error = "Invalid signature" });
            }

            return Ok(new { message = "Webhook processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            // Return 200 to prevent Razorpay from retrying
            return Ok(new { message = "Webhook received but processing failed" });
        }
    }
}
