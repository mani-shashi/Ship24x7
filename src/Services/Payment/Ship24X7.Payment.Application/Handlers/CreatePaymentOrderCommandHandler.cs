using MediatR;
using Ship24X7.Payment.Application.Commands;
using Ship24X7.Payment.Application.DTOs;
using Ship24X7.Payment.Application.Interfaces;
using Ship24X7.Payment.Domain.Entities;

namespace Ship24X7.Payment.Application.Handlers;

/// <summary>
/// Handler for processing CreatePaymentOrder command requests.
/// Implements business logic for creating payment orders in Razorpay and storing them in database.
/// Coordinates with payment order repository and Razorpay service.
/// Implements idempotency to prevent duplicate payment orders for the same request or shipment.
/// Returns payment order details including Razorpay order ID and key for frontend integration.
/// </summary>
public class CreatePaymentOrderCommandHandler : IRequestHandler<CreatePaymentOrderCommand, PaymentOrderResponse>
{
    private readonly IPaymentOrderRepository _paymentOrderRepository;
    private readonly IRazorpayService _razorpayService;

    /// <summary>
    /// Initializes a new instance of the CreatePaymentOrderCommandHandler class.
    /// Sets up dependencies for repository access and Razorpay API integration.
    /// </summary>
    /// <param name="paymentOrderRepository">Repository for payment order data access and persistence</param>
    /// <param name="razorpayService">Service for interacting with Razorpay API to create orders</param>
    public CreatePaymentOrderCommandHandler(
        IPaymentOrderRepository paymentOrderRepository,
        IRazorpayService razorpayService)
    {
        _paymentOrderRepository = paymentOrderRepository;
        _razorpayService = razorpayService;
    }

    /// <summary>
    /// Handles the CreatePaymentOrder command with comprehensive business logic.
    /// Step-by-step logic flow:
    /// 1. Checks idempotency key in database to see if payment order already exists with this key
    /// 2. If found, returns existing payment order details to prevent duplicate creation
    /// 3. Checks if payment order already exists for the shipment ID
    /// 4. If found and status is Pending or Captured, returns existing order (prevents multiple payments for same shipment)
    /// 5. Constructs Razorpay receipt using format "SHIP_{TrackingNumber}" for easy identification
    /// 6. Prepares notes dictionary with shipment_id and tracking_number for Razorpay metadata
    /// 7. Calls Razorpay API to create order with amount (converted to paise), currency, receipt, and notes
    /// 8. If Razorpay API fails, throws InvalidOperationException with service unavailability message
    /// 9. Creates PaymentOrder entity with generated ID, shipment details, Razorpay order ID, amount, currency
    /// 10. Sets status to Pending, stores idempotency key, and records creation timestamp
    /// 11. Saves payment order to database via repository
    /// 12. Returns PaymentOrderResponse with payment order ID, Razorpay order ID, Razorpay key ID, amount, and currency
    /// Frontend uses this response to initialize Razorpay checkout with the order ID and key.
    /// </summary>
    /// <param name="request">Command containing shipment ID, tracking number, amount, currency, and idempotency key</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>PaymentOrderResponse with payment order details for frontend Razorpay integration</returns>
    public async Task<PaymentOrderResponse> Handle(CreatePaymentOrderCommand request, CancellationToken cancellationToken)
    {
        // Check for idempotency - if payment order already exists with this key, return it
        var existingOrder = await _paymentOrderRepository.GetByIdempotencyKeyAsync(request.IdempotencyKey);
        if (existingOrder != null)
        {
            return new PaymentOrderResponse
            {
                PaymentOrderId = existingOrder.Id,
                RazorpayOrderId = existingOrder.RazorpayOrderId,
                RazorpayKeyId = _razorpayService.GetKeyId(),
                Amount = existingOrder.Amount,
                Currency = existingOrder.Currency
            };
        }

        // Check if payment order already exists for this shipment
        var existingShipmentOrder = await _paymentOrderRepository.GetByShipmentIdAsync(request.ShipmentId);
        if (existingShipmentOrder != null && 
            (existingShipmentOrder.Status == PaymentStatus.Pending || existingShipmentOrder.Status == PaymentStatus.Captured))
        {
            return new PaymentOrderResponse
            {
                PaymentOrderId = existingShipmentOrder.Id,
                RazorpayOrderId = existingShipmentOrder.RazorpayOrderId,
                RazorpayKeyId = _razorpayService.GetKeyId(),
                Amount = existingShipmentOrder.Amount,
                Currency = existingShipmentOrder.Currency
            };
        }

        // Create Razorpay order
        var receipt = $"SHIP_{request.TrackingNumber}";
        var notes = new Dictionary<string, string>
        {
            { "shipment_id", request.ShipmentId.ToString() },
            { "tracking_number", request.TrackingNumber }
        };

        string razorpayOrderId;
        try
        {
            razorpayOrderId = await _razorpayService.CreateOrderAsync(
                request.Amount,
                request.Currency,
                receipt,
                notes);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create Razorpay order. Service may be unavailable.", ex);
        }

        // Create payment order entity
        var paymentOrder = new PaymentOrder
        {
            Id = Guid.NewGuid(),
            ShipmentId = request.ShipmentId,
            TrackingNumber = request.TrackingNumber,
            RazorpayOrderId = razorpayOrderId,
            Amount = request.Amount,
            Currency = request.Currency,
            Status = PaymentStatus.Pending,
            IdempotencyKey = request.IdempotencyKey,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.Empty // Will be set by middleware
        };

        await _paymentOrderRepository.AddAsync(paymentOrder);

        return new PaymentOrderResponse
        {
            PaymentOrderId = paymentOrder.Id,
            RazorpayOrderId = razorpayOrderId,
            RazorpayKeyId = _razorpayService.GetKeyId(),
            Amount = request.Amount,
            Currency = request.Currency
        };
    }
}
