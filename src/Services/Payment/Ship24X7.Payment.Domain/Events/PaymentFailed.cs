using Ship24X7.Shared.Domain;

namespace Ship24X7.Payment.Domain.Events;

/// <summary>
/// Domain event raised when PaymentFailed occurs. Used for event-driven architecture and integration.
/// </summary>
public class PaymentFailed : BaseDomainEvent
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
    public string? RazorpayPaymentId { get; set; }
    /// <summary>
    /// Gets or sets the failurereason.
    /// </summary>
    public string FailureReason { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the failedat.
    /// </summary>
    public DateTime FailedAt { get; set; }
}
