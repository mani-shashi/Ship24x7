using MediatR;
using Ship24X7.Payment.Application.DTOs;
using Ship24X7.Payment.Application.Interfaces;
using Ship24X7.Payment.Application.Queries;

namespace Ship24X7.Payment.Application.Handlers;

/// <summary>
/// Handler for processing GetPaymentOrder query requests.
/// Retrieves payment order details by payment order ID from the database.
/// Implements read-only query logic for CQRS pattern.
/// Returns PaymentOrderDto with payment details or null if not found.
/// Used by PaymentController to fetch payment order information.
/// </summary>
public class GetPaymentOrderQueryHandler : IRequestHandler<GetPaymentOrderQuery, PaymentOrderDto?>
{
    private readonly IPaymentOrderRepository _paymentOrderRepository;

    /// <summary>
    /// Initializes a new instance of the GetPaymentOrderQueryHandler class.
    /// Sets up dependency for repository access.
    /// </summary>
    /// <param name="paymentOrderRepository">Repository for payment order data access</param>
    public GetPaymentOrderQueryHandler(IPaymentOrderRepository paymentOrderRepository)
    {
        _paymentOrderRepository = paymentOrderRepository;
    }

    /// <summary>
    /// Handles the GetPaymentOrder query with simple retrieval logic.
    /// Step-by-step logic flow:
    /// 1. Retrieves payment order from database using payment order ID
    /// 2. If payment order not found, returns null
    /// 3. Maps payment order entity to PaymentOrderDto
    /// 4. Includes all payment details: IDs, shipment info, Razorpay IDs, amount, currency, status, timestamps
    /// 5. Converts PaymentStatus enum to string for API response
    /// 6. Returns PaymentOrderDto for controller response
    /// Used by frontend and admin dashboards to check payment status and details.
    /// </summary>
    /// <param name="request">Query containing payment order ID to retrieve</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>PaymentOrderDto with payment details if found; null if not found</returns>
    public async Task<PaymentOrderDto?> Handle(GetPaymentOrderQuery request, CancellationToken cancellationToken)
    {
        var paymentOrder = await _paymentOrderRepository.GetByIdAsync(request.PaymentOrderId);
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
