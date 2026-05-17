using Microsoft.Extensions.Configuration;
using Ship24X7.Payment.Application.Interfaces;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Ship24X7.Payment.Infrastructure.Services;

/// <summary>
/// Service implementation for interacting with Razorpay payment gateway API.
/// Provides functionality for creating payment orders and initiating refunds.
/// Implements IRazorpayService interface for dependency injection.
/// Uses HTTP client with basic authentication (Razorpay key ID and secret).
/// Handles currency conversion (rupees to paise) and API communication.
/// Base URL: https://api.razorpay.com/v1/
/// </summary>
public class RazorpayApiService : IRazorpayService
{
    private readonly HttpClient _httpClient;
    private readonly string _keyId;
    private readonly string _keySecret;

    /// <summary>
    /// Initializes a new instance of the RazorpayApiService class.
    /// Sets up HTTP client with Razorpay API base URL and basic authentication.
    /// Step-by-step initialization:
    /// 1. Retrieves Razorpay key ID and secret from configuration
    /// 2. Validates that both credentials are configured (throws if missing)
    /// 3. Creates base64-encoded authentication token from "keyId:keySecret"
    /// 4. Sets Authorization header with "Basic {token}" for all requests
    /// 5. Sets base address to Razorpay API v1 endpoint
    /// </summary>
    /// <param name="httpClient">HTTP client for making API requests to Razorpay</param>
    /// <param name="configuration">Configuration provider for accessing Razorpay credentials</param>
    public RazorpayApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _keyId = configuration["Razorpay:KeyId"] ?? throw new InvalidOperationException("Razorpay KeyId not configured");
        _keySecret = configuration["Razorpay:KeySecret"] ?? throw new InvalidOperationException("Razorpay KeySecret not configured");

        // Set up basic authentication
        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_keyId}:{_keySecret}"));
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);
        _httpClient.BaseAddress = new Uri("https://api.razorpay.com/v1/");
    }

    /// <summary>
    /// Creates a new payment order in Razorpay payment gateway.
    /// Step-by-step logic flow:
    /// 1. Converts amount from rupees to paise (smallest currency unit) by multiplying by 100
    /// 2. Constructs order request object with amount (in paise), currency, receipt, and notes
    /// 3. Sends POST request to Razorpay "orders" endpoint with JSON payload
    /// 4. Ensures HTTP response is successful (throws HttpRequestException if not)
    /// 5. Deserializes JSON response to extract order ID
    /// 6. Validates order ID is not null or empty
    /// 7. Returns Razorpay order ID for frontend payment integration
    /// Used by CreatePaymentOrderCommandHandler to create orders before payment.
    /// </summary>
    /// <param name="amount">Payment amount in rupees (will be converted to paise)</param>
    /// <param name="currency">Currency code (e.g., "INR", "USD")</param>
    /// <param name="receipt">Receipt identifier for order tracking (e.g., "SHIP_123456789")</param>
    /// <param name="notes">Optional metadata dictionary for order (e.g., shipment_id, tracking_number)</param>
    /// <returns>Razorpay order ID for payment processing</returns>
    public async Task<string> CreateOrderAsync(decimal amount, string currency, string receipt, Dictionary<string, string>? notes = null)
    {
        var amountInPaise = (int)(amount * 100); // Convert to smallest currency unit

        var orderRequest = new
        {
            amount = amountInPaise,
            currency = currency,
            receipt = receipt,
            notes = notes ?? new Dictionary<string, string>()
        };

        var response = await _httpClient.PostAsJsonAsync("orders", orderRequest);
        response.EnsureSuccessStatusCode();

        var orderResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
        var orderId = orderResponse.GetProperty("id").GetString();

        if (string.IsNullOrEmpty(orderId))
        {
            throw new InvalidOperationException("Failed to create Razorpay order");
        }

        return orderId;
    }

    /// <summary>
    /// Initiates a refund for a captured payment in Razorpay payment gateway.
    /// Step-by-step logic flow:
    /// 1. Converts refund amount from rupees to paise (smallest currency unit) by multiplying by 100
    /// 2. Constructs refund request object with amount (in paise) and notes (reason)
    /// 3. Sends POST request to Razorpay "payments/{paymentId}/refund" endpoint with JSON payload
    /// 4. Ensures HTTP response is successful (throws HttpRequestException if not)
    /// 5. Deserializes JSON response to extract refund ID
    /// 6. Validates refund ID is not null or empty
    /// 7. Returns Razorpay refund ID for refund tracking
    /// Used by InitiateRefundCommandHandler to initiate refunds.
    /// Razorpay processes refund asynchronously and sends webhook when completed.
    /// </summary>
    /// <param name="paymentId">Razorpay payment ID to refund (e.g., "pay_XYZ789ABC123")</param>
    /// <param name="amount">Refund amount in rupees (will be converted to paise)</param>
    /// <param name="notes">Optional refund reason or notes</param>
    /// <returns>Razorpay refund ID for tracking refund status</returns>
    public async Task<string> InitiateRefundAsync(string paymentId, decimal amount, string? notes = null)
    {
        var amountInPaise = (int)(amount * 100); // Convert to smallest currency unit

        var refundRequest = new
        {
            amount = amountInPaise,
            notes = notes
        };

        var response = await _httpClient.PostAsJsonAsync($"payments/{paymentId}/refund", refundRequest);
        response.EnsureSuccessStatusCode();

        var refundResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
        var refundId = refundResponse.GetProperty("id").GetString();

        if (string.IsNullOrEmpty(refundId))
        {
            throw new InvalidOperationException("Failed to initiate refund");
        }

        return refundId;
    }

    /// <summary>
    /// Gets the Razorpay key ID for frontend integration.
    /// Frontend needs this key ID to initialize Razorpay checkout.
    /// Returns the public key ID (not the secret).
    /// Used in payment order response for frontend Razorpay SDK initialization.
    /// </summary>
    /// <returns>Razorpay key ID for frontend payment integration</returns>
    public string GetKeyId()
    {
        return _keyId;
    }
}
