using MediatR;
using Ship24X7.Payment.Application.Commands;
using Ship24X7.Payment.Application.Interfaces;
using Ship24X7.Payment.Domain.Entities;
using Ship24X7.Payment.Domain.Events;

namespace Ship24X7.Payment.Application.Handlers;

/// <summary>
/// Handler for processing VerifyPayment command requests.
/// Implements business logic for verifying payment signature and updating payment order status.
/// Coordinates with payment order repository, signature validator, and event publisher.
/// Critical security component: Validates HMAC-SHA256 signature to ensure payment authenticity.
/// Publishes PaymentCaptured domain event for downstream services after successful verification.
/// </summary>
public class VerifyPaymentCommandHandler : IRequestHandler<VerifyPaymentCommand, bool>
{
    private readonly IPaymentOrderRepository _paymentOrderRepository;
    private readonly IPaymentSignatureValidator _signatureValidator;
    private readonly IPaymentEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of the VerifyPaymentCommandHandler class.
    /// Sets up dependencies for repository access, signature validation, and event publishing.
    /// </summary>
    /// <param name="paymentOrderRepository">Repository for payment order data access and updates</param>
    /// <param name="signatureValidator">Service for validating Razorpay payment signatures using HMAC-SHA256</param>
    /// <param name="eventPublisher">Service for publishing domain events to message broker</param>
    public VerifyPaymentCommandHandler(
        IPaymentOrderRepository paymentOrderRepository,
        IPaymentSignatureValidator signatureValidator,
        IPaymentEventPublisher eventPublisher)
    {
        _paymentOrderRepository = paymentOrderRepository;
        _signatureValidator = signatureValidator;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Handles the VerifyPayment command with comprehensive security validation and business logic.
    /// Step-by-step logic flow:
    /// 1. Validates payment signature using HMAC-SHA256 algorithm
    ///    - Computes expected signature from "razorpay_order_id|razorpay_payment_id" using Razorpay key secret
    ///    - Compares computed signature with provided signature (case-insensitive)
    /// 2. If signature invalid, returns false immediately (payment not verified)
    /// 3. Retrieves payment order from database using Razorpay order ID
    /// 4. If payment order not found, throws InvalidOperationException
    /// 5. Updates payment order with Razorpay payment ID and signature
    /// 6. Changes payment order status from Pending to Captured
    /// 7. Records capture timestamp (CapturedAt) as current UTC time
    /// 8. Updates modification timestamp (UpdatedAt)
    /// 9. Saves updated payment order to database
    /// 10. Creates PaymentCaptured domain event with payment details, shipment info, and correlation ID
    /// 11. Publishes event to message broker for downstream services (e.g., shipment service to start processing)
    /// 12. Returns true indicating successful verification and capture
    /// This handler ensures payment authenticity and triggers shipment fulfillment workflow.
    /// </summary>
    /// <param name="request">Command containing Razorpay order ID, payment ID, and signature for verification</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>True if signature is valid and payment is verified; False if signature is invalid</returns>
    public async Task<bool> Handle(VerifyPaymentCommand request, CancellationToken cancellationToken)
    {
        // Validate signature
        var isValid = _signatureValidator.ValidatePaymentSignature(
            request.RazorpayOrderId,
            request.RazorpayPaymentId,
            request.RazorpaySignature);

        if (!isValid)
        {
            return false;
        }

        // Get payment order
        var paymentOrder = await _paymentOrderRepository.GetByRazorpayOrderIdAsync(request.RazorpayOrderId);
        if (paymentOrder == null)
        {
            throw new InvalidOperationException("Payment order not found");
        }

        // Update payment order
        paymentOrder.RazorpayPaymentId = request.RazorpayPaymentId;
        paymentOrder.RazorpaySignature = request.RazorpaySignature;
        paymentOrder.Status = PaymentStatus.Captured;
        paymentOrder.CapturedAt = DateTime.UtcNow;
        paymentOrder.UpdatedAt = DateTime.UtcNow;

        await _paymentOrderRepository.UpdateAsync(paymentOrder);

        // Publish PaymentCaptured event
        var paymentCapturedEvent = new PaymentCaptured
        {
            PaymentOrderId = paymentOrder.Id,
            ShipmentId = paymentOrder.ShipmentId,
            TrackingNumber = paymentOrder.TrackingNumber,
            RazorpayOrderId = paymentOrder.RazorpayOrderId,
            RazorpayPaymentId = request.RazorpayPaymentId,
            Amount = paymentOrder.Amount,
            Currency = paymentOrder.Currency,
            CapturedAt = paymentOrder.CapturedAt.Value,
            CorrelationId = paymentOrder.CorrelationId
        };

        await _eventPublisher.PublishAsync(paymentCapturedEvent);

        return true;
    }
}
