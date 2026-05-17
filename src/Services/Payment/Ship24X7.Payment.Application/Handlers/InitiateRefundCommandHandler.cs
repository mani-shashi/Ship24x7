using MediatR;
using Ship24X7.Payment.Application.Commands;
using Ship24X7.Payment.Application.DTOs;
using Ship24X7.Payment.Application.Interfaces;
using Ship24X7.Payment.Domain.Entities;

namespace Ship24X7.Payment.Application.Handlers;

/// <summary>
/// Handler for processing InitiateRefund command requests.
/// Implements business logic for initiating refunds with Razorpay and storing refund records.
/// Coordinates with payment order repository, refund repository, and Razorpay service.
/// Validates payment order status and refund amount before processing.
/// Returns refund details including Razorpay refund ID for tracking.
/// </summary>
public class InitiateRefundCommandHandler : IRequestHandler<InitiateRefundCommand, RefundResponse>
{
    private readonly IPaymentOrderRepository _paymentOrderRepository;
    private readonly IPaymentRefundRepository _refundRepository;
    private readonly IRazorpayService _razorpayService;

    /// <summary>
    /// Initializes a new instance of the InitiateRefundCommandHandler class.
    /// Sets up dependencies for repository access and Razorpay API integration.
    /// </summary>
    /// <param name="paymentOrderRepository">Repository for payment order data access</param>
    /// <param name="refundRepository">Repository for refund data persistence</param>
    /// <param name="razorpayService">Service for interacting with Razorpay API to initiate refunds</param>
    public InitiateRefundCommandHandler(
        IPaymentOrderRepository paymentOrderRepository,
        IPaymentRefundRepository refundRepository,
        IRazorpayService razorpayService)
    {
        _paymentOrderRepository = paymentOrderRepository;
        _refundRepository = refundRepository;
        _razorpayService = razorpayService;
    }

    /// <summary>
    /// Handles the InitiateRefund command with comprehensive validation and business logic.
    /// Step-by-step logic flow:
    /// 1. Retrieves payment order from database using payment order ID
    /// 2. If payment order not found, throws InvalidOperationException
    /// 3. Validates payment order status is Captured (only captured payments can be refunded)
    /// 4. If status is not Captured, throws InvalidOperationException with status requirement message
    /// 5. Validates refund amount is positive and does not exceed original payment amount
    /// 6. If amount invalid, throws ArgumentException
    /// 7. Calls Razorpay API to initiate refund with payment ID, amount (converted to paise), and reason
    /// 8. If Razorpay API fails, throws InvalidOperationException with service failure message
    /// 9. Creates PaymentRefund entity with generated ID, payment order ID, Razorpay refund ID
    /// 10. Sets refund amount, currency, status to Initiated, reason, and initiator user ID
    /// 11. Records creation timestamp and creator ID
    /// 12. Saves refund record to database via repository
    /// 13. Returns RefundResponse with refund ID, status, amount, currency, and Razorpay refund ID
    /// Webhook will later update refund status to Processed when Razorpay completes the refund.
    /// </summary>
    /// <param name="request">Command containing payment order ID, refund amount, reason, and initiator user ID</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>RefundResponse with refund details for tracking and audit</returns>
    public async Task<RefundResponse> Handle(InitiateRefundCommand request, CancellationToken cancellationToken)
    {
        // Get payment order
        var paymentOrder = await _paymentOrderRepository.GetByIdAsync(request.PaymentOrderId);
        if (paymentOrder == null)
        {
            throw new InvalidOperationException("Payment order not found");
        }

        // Validate payment order status
        if (paymentOrder.Status != PaymentStatus.Captured)
        {
            throw new InvalidOperationException("Payment order must be in Captured status to initiate refund");
        }

        // Validate refund amount
        if (request.Amount <= 0 || request.Amount > paymentOrder.Amount)
        {
            throw new ArgumentException("Invalid refund amount");
        }

        // Initiate refund with Razorpay
        string razorpayRefundId;
        try
        {
            razorpayRefundId = await _razorpayService.InitiateRefundAsync(
                paymentOrder.RazorpayPaymentId!,
                request.Amount,
                request.Reason);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initiate refund with Razorpay", ex);
        }

        // Create refund entity
        var refund = new PaymentRefund
        {
            Id = Guid.NewGuid(),
            PaymentOrderId = paymentOrder.Id,
            RazorpayRefundId = razorpayRefundId,
            Amount = request.Amount,
            Currency = paymentOrder.Currency,
            Status = RefundStatus.Initiated,
            Reason = request.Reason,
            InitiatedBy = request.InitiatedBy,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.InitiatedBy
        };

        await _refundRepository.AddAsync(refund);

        return new RefundResponse
        {
            RefundId = refund.Id,
            Status = refund.Status.ToString(),
            Amount = refund.Amount,
            Currency = refund.Currency,
            RazorpayRefundId = razorpayRefundId
        };
    }
}
