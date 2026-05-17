using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ship24X7.Notification.Application.Interfaces;

namespace Ship24X7.Notification.Infrastructure.Services;

/// <summary>
/// Implements SMS notification delivery using Twilio API for sending text messages.
/// Sends plain text SMS messages through Twilio's messaging service with delivery tracking.
/// Supports international phone numbers in E.164 format (+country code).
/// Used by SendNotificationCommandHandler to deliver SMS channel notifications.
/// Currently implements stub/mock functionality - TODO: integrate actual Twilio SDK for production use.
/// </summary>
public class TwilioSmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioSmsService> _logger;
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromNumber;

    /// <summary>
    /// Initializes a new instance of the <see cref="TwilioSmsService"/> class.
    /// Loads Twilio configuration from appsettings.json including account credentials and sender phone number.
    /// Logic: Reads Twilio:AccountSid (required, throws if missing), Twilio:AuthToken (required, throws if missing),
    /// Twilio:FromNumber (required, throws if missing - must be Twilio-verified phone number).
    /// Configuration is validated at startup to fail fast if Twilio credentials are missing.
    /// </summary>
    /// <param name="configuration">Application configuration containing Twilio settings under Twilio section.</param>
    /// <param name="logger">Logger for recording SMS sending operations, successes, and failures.</param>
    /// <exception cref="InvalidOperationException">Thrown when Twilio AccountSid, AuthToken, or FromNumber is not configured in appsettings.</exception>
    public TwilioSmsService(IConfiguration configuration, ILogger<TwilioSmsService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _accountSid = _configuration["Twilio:AccountSid"] ?? throw new InvalidOperationException("Twilio AccountSid not configured");
        _authToken = _configuration["Twilio:AuthToken"] ?? throw new InvalidOperationException("Twilio AuthToken not configured");
        _fromNumber = _configuration["Twilio:FromNumber"] ?? throw new InvalidOperationException("Twilio FromNumber not configured");
    }

    /// <summary>
    /// Sends a plain text SMS message to a single recipient using Twilio API.
    /// Logic flow (TODO - currently stub): 1) Initialize Twilio client with AccountSid and AuthToken,
    /// 2) Create message resource with From (Twilio number), To (recipient), and Body (message text),
    /// 3) Send message via Twilio REST API, 4) Check response status (queued/sent/failed),
    /// 5) Log success or catch exceptions (invalid number, insufficient balance, API errors),
    /// 6) Return true if queued/sent successfully, false if any error occurs.
    /// Current implementation: Logs message and simulates async operation for testing without Twilio account.
    /// </summary>
    /// <param name="to">Recipient's phone number in E.164 format (+country code, e.g., +919876543210). Must be valid mobile number.</param>
    /// <param name="message">SMS message text in plain format. No HTML tags. Keep under 160 characters to avoid multi-part messages and extra charges.</param>
    /// <param name="cancellationToken">Cancellation token to abort SMS sending operation if request is cancelled.</param>
    /// <returns>
    /// True if SMS was queued/sent successfully to Twilio (does not guarantee delivery to recipient's phone).
    /// False if any error occurred: invalid phone number, insufficient Twilio balance, API rate limit, network error, etc.
    /// </returns>
    public async Task<bool> SendSmsAsync(string to, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            // TODO: Implement actual Twilio SMS sending using Twilio SDK
            // Steps: 1) Install Twilio NuGet package, 2) Initialize TwilioClient with _accountSid and _authToken,
            // 3) Call MessageResource.CreateAsync with from: _fromNumber, to: to, body: message,
            // 4) Check message.Status (queued/sent/failed), 5) Return true if queued/sent, false if failed
            
            // For now, just log the SMS for testing without Twilio account
            _logger.LogInformation("SMS would be sent to {To}: {Message}", to, message);
            
            // Simulate async operation to match real Twilio API call behavior
            await Task.Delay(100, cancellationToken);
            
            return true; // Simulate successful send for testing
        }
        catch (Exception ex)
        {
            // Catch all exceptions: TwilioException (API errors), HttpException (network errors), etc.
            _logger.LogError(ex, "Failed to send SMS to {To}", to);
            return false; // Indicate failure for notification log update
        }
    }
}
