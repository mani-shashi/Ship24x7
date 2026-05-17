namespace Ship24X7.Payment.Application.DTOs;

/// <summary>
/// Data transfer object for PaymentOrder data. Used for API responses and data serialization.
/// </summary>
public class PaymentOrderResponse
{
    /// <summary>
    /// Gets or sets the Payment orderId.
    /// </summary>
    public Guid PaymentOrderId { get; set; }
    /// <summary>
    /// Gets or sets the Razorpay orderId.
    /// </summary>
    public string RazorpayOrderId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Razorpay keyId.
    /// </summary>
    public string RazorpayKeyId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// Gets or sets the currency.
    /// </summary>
    public string Currency { get; set; } = string.Empty;
}
