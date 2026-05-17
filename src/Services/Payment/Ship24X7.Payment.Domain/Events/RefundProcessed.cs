using Ship24X7.Shared.Domain;

namespace Ship24X7.Payment.Domain.Events;

/// <summary>
/// Domain event raised when RefundProcessed occurs. Used for event-driven architecture and integration.
/// </summary>
public class RefundProcessed : BaseDomainEvent
{
    /// <summary>
    /// Gets or sets the refundId.
    /// </summary>
    public Guid RefundId { get; set; }
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
    /// Gets or sets the Razorpay refundId.
    /// </summary>
    public string RazorpayRefundId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// Gets or sets the currency.
    /// </summary>
    public string Currency { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the processedat.
    /// </summary>
    public DateTime ProcessedAt { get; set; }
}
