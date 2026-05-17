using MediatR;

namespace Ship24X7.Payment.Application.Commands;

/// <summary>
/// Command for processing incoming webhook notifications from Razorpay payment gateway.
/// Encapsulates webhook event data including event type, payload, and signature for validation.
/// Implements IRequest pattern from MediatR for CQRS architecture.
/// Returns boolean indicating whether webhook signature is valid and event was processed successfully.
/// Handles three event types: payment.captured, payment.failed, and refund.processed.
/// Critical for maintaining payment state consistency between Razorpay and Ship24X7 system.
/// </summary>
public class ProcessWebhookCommand : IRequest<bool>
{
    /// <summary>
    /// Gets or sets the type of webhook event received from Razorpay.
    /// Determines which processing logic to execute.
    /// Supported values:
    /// - "payment.captured": Payment successfully captured by Razorpay
    /// - "payment.failed": Payment attempt failed
    /// - "refund.processed": Refund successfully processed by Razorpay
    /// Required field. Example: "payment.captured"
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw JSON payload received from Razorpay webhook.
    /// Contains complete event data including entity details (payment/refund), amounts, IDs, and timestamps.
    /// Used for signature validation and extracting event-specific information.
    /// Required field. Must be valid JSON string.
    /// </summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HMAC-SHA256 signature from X-Razorpay-Signature header.
    /// Used to validate webhook authenticity and prevent tampering.
    /// Computed by Razorpay using webhook secret and payload.
    /// Handler validates this signature before processing the event.
    /// Required field. Example: "a1b2c3d4e5f6..."
    /// </summary>
    public string Signature { get; set; } = string.Empty;
}
