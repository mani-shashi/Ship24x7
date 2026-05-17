using MediatR;
using Ship24X7.Payment.Application.DTOs;
using Ship24X7.Payment.Application.Interfaces;
using Ship24X7.Payment.Application.Queries;

namespace Ship24X7.Payment.Application.Handlers;

/// <summary>
/// Handler for processing GetPaymentOrderByShipment query requests.
/// Retrieves payment order details by shipment ID from the database.
/// Implements read-only query logic for CQRS pattern.
/// Returns PaymentOrderDto with payment details or null if not found.
/// Used by PaymentController and shipment service to fetch payment information for a specific shipment.
/// </summary>
public class GetPaymentOrderByShipmentQueryHandler : IRequestHandler<GetPaymentOrderByShipmentQuery, PaymentOrderDto?>
{
    private readonly IPaymentOrderRepository _paymentOrderRepository;

    /// <summary>
    /// Initializes a new instance of the GetPaymentOrderByShipmentQueryHandler class.
    /// Sets up dependency for repository access.
    /// </summary>
    /// <param name="paymentOrderRepository">Repository for payment order data access</param>
    public GetPaymentOrderByShipmentQueryHandler(IPaymentOrderRepository paymentOrderRepository)
    {
        _paymentOrderRepository = paymentOrderRepository;
    }

    /// <summary>
    /// Handles the GetPaymentOrderByShipment query with simple retrieval logic.
    /// Step-by-step logic flow:
    /// 1. Retrieves payment order from database using shipment ID
    /// 2. If payment order not found, returns null
    /// 3. Maps payment order entity to PaymentOrderDto
    /// 4. Includes all payment details: IDs, shipment info, Razorpay IDs, amount, currency, status, timestamps
    /// 5. Converts PaymentStatus enum to string for API response
    /// 6. Returns PaymentOrderDto for controller response
    /// Used by frontend to display payment information on shipment tracking pages.
    /// Used by shipment service to check payment status before processing shipment.
    /// </summary>
    /// <param name="request">Query containing shipment ID to retrieve payment order for</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>PaymentOrderDto with payment details if found; null if not found</returns>
    public async Task<PaymentOrderDto?> Handle(GetPaymentOrderByShipmentQuery request, CancellationToken cancellationToken)
    {
        var paymentOrder = await _paymentOrderRepository.GetByShipmentIdAsync(request.ShipmentId);
        if (paymentOrder == null)
        {
            return null;
        }

        return new PaymentOrderDto
        {
            Id = paymentOrder.Id,
            ShipmentId = paymentOrder.ShipmentId,
            TrackingNumber = paymentOrder.TrackingNumber,
            RazorpayOrderId = paymentOrder.RazorpayOrderId,
            RazorpayPaymentId = paymentOrder.RazorpayPaymentId,
            Amount = paymentOrder.Amount,
            Currency = paymentOrder.Currency,
            Status = paymentOrder.Status.ToString(),
            CapturedAt = paymentOrder.CapturedAt,
            FailureReason = paymentOrder.FailureReason,
            CreatedAt = paymentOrder.CreatedAt
        };
    }
}
