using Microsoft.Extensions.Configuration;
using Ship24X7.Payment.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Ship24X7.Payment.Infrastructure.Services;

/// <summary>
/// Service for validating HMAC-SHA256 signatures from Razorpay payment gateway.
/// Implements both payment signature validation and webhook signature validation.
/// Ensures payment authenticity and prevents tampering by verifying cryptographic signatures.
/// Uses Razorpay key secret for payment signatures and webhook secret for webhook signatures.
/// Critical security component for payment verification and webhook processing.
/// </summary>
public class HmacSignatureValidator : IPaymentSignatureValidator, IWebhookSignatureValidator
{
    private readonly string _keySecret;
    private readonly string _webhookSecret;

    /// <summary>
    /// Initializes a new instance of the HmacSignatureValidator class.
    /// Retrieves Razorpay key secret and webhook secret from configuration.
    /// Validates that both secrets are configured (throws if missing).
    /// </summary>
    /// <param name="configuration">Configuration provider for accessing Razorpay secrets</param>
    public HmacSignatureValidator(IConfiguration configuration)
    {
        _keySecret = configuration["Razorpay:KeySecret"] ?? throw new InvalidOperationException("Razorpay KeySecret not configured");
        _webhookSecret = configuration["Razorpay:WebhookSecret"] ?? throw new InvalidOperationException("Razorpay WebhookSecret not configured");
    }

    /// <summary>
    /// Validates payment signature after successful payment on Razorpay frontend.
    /// Step-by-step logic flow:
    /// 1. Constructs message string by concatenating "razorpay_order_id|razorpay_payment_id"
    /// 2. Computes HMAC-SHA256 hash of message using Razorpay key secret
    /// 3. Converts computed hash to lowercase hexadecimal string
    /// 4. Compares computed signature with provided signature (case-insensitive)
    /// 5. Returns true if signatures match, false otherwise
    /// This ensures payment was actually processed by Razorpay and not tampered with.
    /// Used by VerifyPaymentCommandHandler to validate payment authenticity.
    /// </summary>
    /// <param name="razorpayOrderId">Razorpay order ID from payment order creation</param>
    /// <param name="razorpayPaymentId">Razorpay payment ID from payment completion</param>
    /// <param name="razorpaySignature">Signature provided by Razorpay after payment</param>
    /// <returns>True if signature is valid; False if signature is invalid or tampered</returns>
    public bool ValidatePaymentSignature(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature)
    {
        var message = $"{razorpayOrderId}|{razorpayPaymentId}";
        var expectedSignature = ComputeHmacSha256(message, _keySecret);
        return expectedSignature.Equals(razorpaySignature, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates webhook signature from Razorpay webhook notifications.
    /// Step-by-step logic flow:
    /// 1. Computes HMAC-SHA256 hash of raw webhook payload using webhook secret
    /// 2. Converts computed hash to lowercase hexadecimal string
    /// 3. Compares computed signature with X-Razorpay-Signature header value (case-insensitive)
    /// 4. Returns true if signatures match, false otherwise
    /// This ensures webhook was actually sent by Razorpay and payload was not tampered with.
    /// Used by ProcessWebhookCommandHandler to validate webhook authenticity.
    /// </summary>
    /// <param name="payload">Raw JSON webhook payload from Razorpay</param>
    /// <param name="signature">Signature from X-Razorpay-Signature header</param>
    /// <returns>True if signature is valid; False if signature is invalid or tampered</returns>
    public bool ValidateWebhookSignature(string payload, string signature)
    {
        var expectedSignature = ComputeHmacSha256(payload, _webhookSecret);
        return expectedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Computes HMAC-SHA256 hash of a message using a secret key.
    /// Step-by-step logic flow:
    /// 1. Converts secret key to UTF-8 byte array
    /// 2. Converts message to UTF-8 byte array
    /// 3. Creates HMACSHA256 instance with secret key bytes
    /// 4. Computes hash of message bytes
    /// 5. Converts hash bytes to hexadecimal string
    /// 6. Converts hexadecimal string to lowercase
    /// 7. Returns lowercase hexadecimal hash string
    /// Used internally by both payment and webhook signature validation methods.
    /// </summary>
    /// <param name="message">Message to hash (order_id|payment_id or webhook payload)</param>
    /// <param name="secret">Secret key for HMAC computation (key secret or webhook secret)</param>
    /// <returns>Lowercase hexadecimal HMAC-SHA256 hash string</returns>
    private static string ComputeHmacSha256(string message, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
