namespace Ship24X7.Payment.Application.DTOs;

/// <summary>
/// Data transfer object for RefundDto data. Used for API responses and data serialization.
/// </summary>
public class RefundDto
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets the Payment orderId.
    /// </summary>
    public Guid PaymentOrderId { get; set; }
    /// <summary>
    /// Gets or sets the Razorpay refundId.
    /// </summary>
    public string? RazorpayRefundId { get; set; }
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
    /// Gets or sets the processedat.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
    /// <summary>
    /// Gets or sets the reason.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the initiatedby.
    /// </summary>
    public Guid InitiatedBy { get; set; }
    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
