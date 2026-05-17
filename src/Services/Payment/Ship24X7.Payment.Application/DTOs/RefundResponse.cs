namespace Ship24X7.Payment.Application.DTOs;

/// <summary>
/// Data transfer object for Refund data. Used for API responses and data serialization.
/// </summary>
public class RefundResponse
{
    /// <summary>
    /// Gets or sets the refundId.
    /// </summary>
    public Guid RefundId { get; set; }
    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// Gets or sets the currency.
    /// </summary>
    public string Currency { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Razorpay refundId.
    /// </summary>
    public string? RazorpayRefundId { get; set; }
}
