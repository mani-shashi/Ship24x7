using MediatR;
using Ship24X7.Payment.Application.Commands;
using Ship24X7.Payment.Application.Interfaces;
using Ship24X7.Payment.Domain.Entities;
using Ship24X7.Payment.Domain.Events;
using System.Text.Json;

namespace Ship24X7.Payment.Application.Handlers;

/// <summary>
/// Handler for processing Razorpay webhook notifications.
/// Implements business logic for handling payment.captured, payment.failed, and refund.processed events.
/// Coordinates with payment order repository, refund repository, webhook validator, and event publisher.
/// Critical for maintaining payment state consistency between Razorpay and Ship24X7 system.
/// Validates webhook signature before processing to ensure authenticity and prevent tampering.
/// </summary>
public class ProcessWebhookCommandHandler : IRequestHandler<ProcessWebhookCommand, bool>
{
    private readonly IPaymentOrderRepository _paymentOrderRepository;
    private readonly IPaymentRefundRepository _refundRepository;
    private readonly IWebhookSignatureValidator _webhookValidator;
    private readonly IPaymentEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of the ProcessWebhookCommandHandler class.
    /// Sets up dependencies for repository access, signature validation, and event publishing.
    /// </summary>
    /// <param name="paymentOrderRepository">Repository for payment order data access and updates</param>
    /// <param name="refundRepository">Repository for refund data access and updates</param>
    /// <param name="webhookValidator">Service for validating Razorpay webhook signatures using HMAC-SHA256</param>
    /// <param name="eventPublisher">Service for publishing domain events to message broker</param>
    public ProcessWebhookCommandHandler(
        IPaymentOrderRepository paymentOrderRepository,
        IPaymentRefundRepository refundRepository,
        IWebhookSignatureValidator webhookValidator,
        IPaymentEventPublisher eventPublisher)
    {
        _paymentOrderRepository = paymentOrderRepository;
        _refundRepository = refundRepository;
        _webhookValidator = webhookValidator;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Handles the ProcessWebhook command with comprehensive security validation and event routing.
    /// Step-by-step logic flow:
    /// 1. Validates webhook signature using HMAC-SHA256 with webhook secret
    /// 2. If signature invalid, returns false immediately (webhook rejected)
    /// 3. Parses JSON payload to extract webhook data
    /// 4. Routes to appropriate handler based on event type:
    ///    - "payment.captured": Calls HandlePaymentCaptured
    ///    - "payment.failed": Calls HandlePaymentFailed
    ///    - "refund.processed": Calls HandleRefundProcessed
    ///    - Unknown event types: Returns true to acknowledge (prevents Razorpay retries)
    /// 5. Returns true indicating successful processing
    /// Each event handler updates database and publishes domain events for downstream services.
    /// </summary>
    /// <param name="request">Command containing event type, JSON payload, and signature</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>True if signature is valid and event is processed; False if signature is invalid</returns>
    public async Task<bool> Handle(ProcessWebhookCommand request, CancellationToken cancellationToken)
    {
        // Validate webhook signature
        if (!_webhookValidator.ValidateWebhookSignature(request.Payload, request.Signature))
        {
            return false;
        }

        // Parse webhook payload
        var webhookData = JsonSerializer.Deserialize<JsonElement>(request.Payload);
        var entity = webhookData.GetProperty("entity").GetString() ?? string.Empty;

        switch (request.EventType)
        {
            case "payment.captured":
                await HandlePaymentCaptured(webhookData);
                break;

            case "payment.failed":
                await HandlePaymentFailed(webhookData);
                break;

            case "refund.processed":
                await HandleRefundProcessed(webhookData);
                break;

            default:
                // Unknown event type, log and return true to acknowledge
                return true;
        }

        return true;
    }

    /// <summary>
    /// Handles payment.captured webhook event from Razorpay.
    /// Step-by-step logic flow:
    /// 1. Extracts payment entity from webhook payload JSON structure
    /// 2. Retrieves Razorpay order ID, payment ID, and amount from payment entity
    /// 3. Converts amount from paise to rupees (divides by 100)
    /// 4. Fetches payment order from database using Razorpay order ID
    /// 5. If payment order not found or already captured, returns early (idempotency)
    /// 6. Updates payment order with Razorpay payment ID
    /// 7. Changes status from Pending to Captured
    /// 8. Records capture timestamp as current UTC time
    /// 9. Updates modification timestamp
    /// 10. Saves updated payment order to database
    /// 11. Creates PaymentCaptured domain event with payment details and shipment info
    /// 12. Publishes event to message broker for downstream services (e.g., shipment service)
    /// This ensures payment capture is recorded and triggers shipment fulfillment workflow.
    /// </summary>
    /// <param name="webhookData">Parsed JSON webhook data containing payment capture details</param>
    private async Task HandlePaymentCaptured(JsonElement webhookData)
    {
        var payload = webhookData.GetProperty("payload");
        var payment = payload.GetProperty("payment").GetProperty("entity");
        
        var orderId = payment.GetProperty("order_id").GetString() ?? string.Empty;
        var paymentId = payment.GetProperty("id").GetString() ?? string.Empty;
        var amount = payment.GetProperty("amount").GetInt64() / 100m; // Convert paise to rupees

        var paymentOrder = await _paymentOrderRepository.GetByRazorpayOrderIdAsync(orderId);
        if (paymentOrder == null || paymentOrder.Status == PaymentStatus.Captured)
        {
            return; // Already processed or not found
        }

        paymentOrder.RazorpayPaymentId = paymentId;
        paymentOrder.Status = PaymentStatus.Captured;
        paymentOrder.CapturedAt = DateTime.UtcNow;
        paymentOrder.UpdatedAt = DateTime.UtcNow;

        await _paymentOrderRepository.UpdateAsync(paymentOrder);

        // Publish event
        var paymentCapturedEvent = new PaymentCaptured
        {
            PaymentOrderId = paymentOrder.Id,
            ShipmentId = paymentOrder.ShipmentId,
            TrackingNumber = paymentOrder.TrackingNumber,
            RazorpayOrderId = paymentOrder.RazorpayOrderId,
            RazorpayPaymentId = paymentId,
            Amount = paymentOrder.Amount,
            Currency = paymentOrder.Currency,
            CapturedAt = paymentOrder.CapturedAt.Value,
            CorrelationId = paymentOrder.CorrelationId
        };

        await _eventPublisher.PublishAsync(paymentCapturedEvent);
    }

    /// <summary>
    /// Handles payment.failed webhook event from Razorpay.
    /// Step-by-step logic flow:
    /// 1. Extracts payment entity from webhook payload JSON structure
    /// 2. Retrieves Razorpay order ID, payment ID, and error description from payment entity
    /// 3. Fetches payment order from database using Razorpay order ID
    /// 4. If payment order not found, returns early
    /// 5. Updates payment order with Razorpay payment ID
    /// 6. Changes status from Pending to Failed
    /// 7. Records failure reason from error description
    /// 8. Updates modification timestamp
    /// 9. Saves updated payment order to database
    /// 10. Creates PaymentFailed domain event with payment details and failure reason
    /// 11. Publishes event to message broker for downstream services (e.g., notification service to alert customer)
    /// This ensures payment failures are recorded and customers are notified to retry payment.
    /// </summary>
    /// <param name="webhookData">Parsed JSON webhook data containing payment failure details</param>
    private async Task HandlePaymentFailed(JsonElement webhookData)
    {
        var payload = webhookData.GetProperty("payload");
        var payment = payload.GetProperty("payment").GetProperty("entity");
        
        var orderId = payment.GetProperty("order_id").GetString() ?? string.Empty;
        var paymentId = payment.GetProperty("id").GetString() ?? string.Empty;
        var errorDescription = payment.GetProperty("error_description").GetString() ?? "Payment failed";

        var paymentOrder = await _paymentOrderRepository.GetByRazorpayOrderIdAsync(orderId);
        if (paymentOrder == null)
        {
            return;
        }

        paymentOrder.RazorpayPaymentId = paymentId;
        paymentOrder.Status = PaymentStatus.Failed;
        paymentOrder.FailureReason = errorDescription;
        paymentOrder.UpdatedAt = DateTime.UtcNow;

        await _paymentOrderRepository.UpdateAsync(paymentOrder);

        // Publish event
        var paymentFailedEvent = new PaymentFailed
        {
            PaymentOrderId = paymentOrder.Id,
            ShipmentId = paymentOrder.ShipmentId,
            TrackingNumber = paymentOrder.TrackingNumber,
            RazorpayOrderId = paymentOrder.RazorpayOrderId,
            RazorpayPaymentId = paymentId,
            FailureReason = errorDescription,
            FailedAt = DateTime.UtcNow,
            CorrelationId = paymentOrder.CorrelationId
        };

        await _eventPublisher.PublishAsync(paymentFailedEvent);
    }

    /// <summary>
    /// Handles refund.processed webhook event from Razorpay.
    /// Step-by-step logic flow:
    /// 1. Extracts refund entity from webhook payload JSON structure
    /// 2. Retrieves Razorpay refund ID and amount from refund entity
    /// 3. Converts amount from paise to rupees (divides by 100)
    /// 4. Fetches payment refund from database using Razorpay refund ID
    /// 5. If refund not found or already processed, returns early (idempotency)
    /// 6. Updates refund status from Initiated to Processed
    /// 7. Records processed timestamp as current UTC time
    /// 8. Updates modification timestamp
    /// 9. Saves updated refund to database
    /// 10. Fetches associated payment order using payment order ID from refund
    /// 11. If payment order found, updates its status from Captured to Refunded
    /// 12. Updates payment order modification timestamp
    /// 13. Saves updated payment order to database
    /// 14. Creates RefundProcessed domain event with refund details and shipment info
    /// 15. Publishes event to message broker for downstream services (e.g., notification service to alert customer)
    /// This ensures refund completion is recorded and customers are notified of successful refund.
    /// </summary>
    /// <param name="webhookData">Parsed JSON webhook data containing refund processing details</param>
    private async Task HandleRefundProcessed(JsonElement webhookData)
    {
        var payload = webhookData.GetProperty("payload");
        var refund = payload.GetProperty("refund").GetProperty("entity");
        
        var refundId = refund.GetProperty("id").GetString() ?? string.Empty;
        var amount = refund.GetProperty("amount").GetInt64() / 100m; // Convert paise to rupees

        var paymentRefund = await _refundRepository.GetByRazorpayRefundIdAsync(refundId);
        if (paymentRefund == null || paymentRefund.Status == RefundStatus.Processed)
        {
            return; // Already processed or not found
        }

        paymentRefund.Status = RefundStatus.Processed;
        paymentRefund.ProcessedAt = DateTime.UtcNow;
        paymentRefund.UpdatedAt = DateTime.UtcNow;

        await _refundRepository.UpdateAsync(paymentRefund);

        // Get payment order for event
        var paymentOrder = await _paymentOrderRepository.GetByIdAsync(paymentRefund.PaymentOrderId);
        if (paymentOrder != null)
        {
            // Update payment order status to Refunded
            paymentOrder.Status = PaymentStatus.Refunded;
            paymentOrder.UpdatedAt = DateTime.UtcNow;
            await _paymentOrderRepository.UpdateAsync(paymentOrder);

            // Publish event
            var refundProcessedEvent = new RefundProcessed
            {
                RefundId = paymentRefund.Id,
                PaymentOrderId = paymentOrder.Id,
                ShipmentId = paymentOrder.ShipmentId,
                TrackingNumber = paymentOrder.TrackingNumber,
                RazorpayRefundId = refundId,
                Amount = paymentRefund.Amount,
                Currency = paymentRefund.Currency,
                ProcessedAt = paymentRefund.ProcessedAt.Value,
                CorrelationId = paymentOrder.CorrelationId
            };

            await _eventPublisher.PublishAsync(refundProcessedEvent);
        }
    }
}
