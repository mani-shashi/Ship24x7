namespace Ship24X7.Payment.Application.DTOs;

/// <summary>
/// Data transfer object for PaymentOrderDto data. Used for API responses and data serialization.
/// </summary>
public class PaymentOrderDto
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }
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
    /// Gets or sets the amount.
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// Gets or sets the currency.
    /// </summary>
    public string Currency { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the capturedat.
    /// </summary>
    public DateTime? CapturedAt { get; set; }
    /// <summary>
    /// Gets or sets the failurereason.
    /// </summary>
    public string? FailureReason { get; set; }
    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
