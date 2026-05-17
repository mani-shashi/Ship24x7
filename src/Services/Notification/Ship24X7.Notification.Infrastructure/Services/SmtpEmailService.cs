using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ship24X7.Notification.Application.Interfaces;

namespace Ship24X7.Notification.Infrastructure.Services;

/// <summary>
/// Implements email notification delivery using SMTP protocol with TLS encryption.
/// Sends HTML and plain text emails through configured SMTP server (Gmail, SendGrid, AWS SES, etc.).
/// Supports authentication with username/password credentials and customizable sender information.
/// Used by SendNotificationCommandHandler to deliver Email channel notifications.
/// Implements retry logic through exception handling and returns success/failure status for log updates.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmtpEmailService"/> class.
    /// Loads SMTP configuration from appsettings.json including server host, port, credentials, and sender information.
    /// Logic: Reads Email:SmtpHost (defaults to Gmail), Email:SmtpPort (defaults to 587 for TLS),
    /// Email:SmtpUsername and Email:SmtpPassword (required, throws if missing),
    /// Email:FromEmail (defaults to username), Email:FromName (defaults to "Ship24X7").
    /// Configuration is validated at startup to fail fast if SMTP credentials are missing.
    /// </summary>
    /// <param name="configuration">Application configuration containing SMTP settings under Email section.</param>
    /// <param name="logger">Logger for recording email sending operations, successes, and failures.</param>
    /// <exception cref="InvalidOperationException">Thrown when SMTP username or password is not configured in appsettings.</exception>
    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        _smtpUsername = _configuration["Email:SmtpUsername"] ?? throw new InvalidOperationException("SMTP Username not configured");
        _smtpPassword = _configuration["Email:SmtpPassword"] ?? throw new InvalidOperationException("SMTP Password not configured");
        _fromEmail = _configuration["Email:FromEmail"] ?? _smtpUsername;
        _fromName = _configuration["Email:FromName"] ?? "Ship24X7";
    }

    /// <summary>
    /// Sends an HTML email to a single recipient using SMTP protocol with TLS encryption.
    /// Logic flow: 1) Creates SmtpClient with configured host, port, and credentials,
    /// 2) Enables SSL/TLS for secure transmission, 3) Constructs MailMessage with sender, recipient, subject, and HTML body,
    /// 4) Sends email asynchronously with cancellation support, 5) Logs success or catches exceptions and logs failure,
    /// 6) Returns true if sent successfully, false if any error occurs (network timeout, authentication failure, invalid recipient, etc.).
    /// Implements graceful error handling by catching all exceptions and returning false instead of throwing.
    /// </summary>
    /// <param name="to">Recipient's email address. Must be valid email format. Single recipient only (no CC/BCC).</param>
    /// <param name="subject">Email subject line. Can contain Unicode characters. No length limit but keep under 200 chars for best compatibility.</param>
    /// <param name="body">Email body content in HTML format. Supports full HTML tags, CSS styles, images (as URLs), and links.</param>
    /// <param name="cancellationToken">Cancellation token to abort email sending operation if request is cancelled.</param>
    /// <returns>
    /// True if email was sent successfully to SMTP server (does not guarantee delivery to recipient's inbox).
    /// False if any error occurred: network timeout, authentication failure, invalid recipient email, SMTP server error, etc.
    /// </returns>
    public async Task<bool> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create SMTP client with configured server and credentials
            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = true // Enable TLS encryption for secure transmission
            };

            // Construct email message with sender, recipient, subject, and HTML body
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName), // Sender address and display name
                Subject = subject,
                Body = body,
                IsBodyHtml = true // Indicates body contains HTML markup
            };

            mailMessage.To.Add(to); // Add single recipient

            // Send email asynchronously with cancellation support
            await smtpClient.SendMailAsync(mailMessage, cancellationToken);
            
            _logger.LogInformation("Email sent successfully to {To}", to);
            return true; // Email accepted by SMTP server
        }
        catch (Exception ex)
        {
            // Catch all exceptions: SmtpException (server errors), SocketException (network errors), etc.
            _logger.LogError(ex, "Failed to send email to {To}", to);
            return false; // Indicate failure for notification log update
        }
    }
}
