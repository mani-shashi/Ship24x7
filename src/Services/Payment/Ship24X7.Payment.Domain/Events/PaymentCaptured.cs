using Ship24X7.Shared.Domain;

namespace Ship24X7.Payment.Domain.Events;

/// <summary>
/// Domain event raised when PaymentCaptured occurs. Used for event-driven architecture and integration.
/// </summary>
public class PaymentCaptured : BaseDomainEvent
{
    /// <summary>
    /// Gets or sets the Payment orderId.
    /// </summary>
    public Guid PaymentOrderId { get; set; }
    /// <summary>
    /// Gets or sets the shipmentId.
    /// </summary>
    public Guid ShipmentId { get; set; }
    /// <summary>
    /// Gets or sets the trackingNumber.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Razorpay orderId.
    /// </summary>
    public string RazorpayOrderId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Razorpay paymentId.
    /// </summary>
    public string RazorpayPaymentId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// Gets or sets the currency.
    /// </summary>
    public string Currency { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the capturedat.
    /// </summary>
    public DateTime CapturedAt { get; set; }
}
