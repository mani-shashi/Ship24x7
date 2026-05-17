namespace Ship24X7.Payment.Application.Interfaces;

/// <summary>
/// IRazorpay service implementation. Provides irazorpay functionality for the application.
/// </summary>
public interface IRazorpayService
{
    Task<string> CreateOrderAsync(decimal amount, string currency, string receipt, Dictionary<string, string>? notes = null);
    Task<string> InitiateRefundAsync(string paymentId, decimal amount, string? notes = null);
    string GetKeyId();
}
