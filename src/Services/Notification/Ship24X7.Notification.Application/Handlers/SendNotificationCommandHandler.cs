using MediatR;
using Microsoft.Extensions.Logging;
using Ship24X7.Notification.Application.Commands;
using Ship24X7.Notification.Application.Interfaces;
using Ship24X7.Notification.Domain.Entities;
using Ship24X7.Notification.Domain.ValueObjects;

namespace Ship24X7.Notification.Application.Handlers;

/// <summary>
/// Handles the SendNotificationCommand by orchestrating template retrieval, preference checking, template rendering, and multi-channel delivery.
/// Implements the core notification sending logic with the following workflow:
/// 1) Validates template exists and is active, 2) Checks user preferences to respect channel opt-outs,
/// 3) Renders template with placeholder data using Razor engine, 4) Creates notification log for audit trail,
/// 5) Dispatches to appropriate delivery service (SMTP/Twilio), 6) Updates log with delivery status and timestamp.
/// Supports Email and SMS channels with extensibility for Push notifications. Implements retry logic through status tracking.
/// </summary>
public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, Guid>
{
    private readonly INotificationLogRepository _notificationLogRepository;
    private readonly INotificationTemplateRepository _templateRepository;
    private readonly INotificationPreferenceRepository _preferenceRepository;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly ILogger<SendNotificationCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SendNotificationCommandHandler"/> class.
    /// Injects all required repositories and services for notification processing pipeline.
    /// </summary>
    /// <param name="notificationLogRepository">Repository for persisting notification delivery logs and status updates.</param>
    /// <param name="templateRepository">Repository for retrieving notification templates with subject and body content.</param>
    /// <param name="preferenceRepository">Repository for checking user notification preferences and channel opt-outs.</param>
    /// <param name="emailService">SMTP service for sending email notifications through configured mail server.</param>
    /// <param name="smsService">Twilio service for sending SMS notifications through Twilio API.</param>
    /// <param name="templateRenderer">Razor template rendering service for replacing {{placeholders}} with actual values.</param>
    /// <param name="logger">Logger for recording notification processing events, errors, and user opt-out decisions.</param>
    public SendNotificationCommandHandler(
        INotificationLogRepository notificationLogRepository,
        INotificationTemplateRepository templateRepository,
        INotificationPreferenceRepository preferenceRepository,
        IEmailService emailService,
        ISmsService smsService,
        ITemplateRenderer templateRenderer,
        ILogger<SendNotificationCommandHandler> logger)
    {
        _notificationLogRepository = notificationLogRepository;
        _templateRepository = templateRepository;
        _preferenceRepository = preferenceRepository;
        _emailService = emailService;
        _smsService = smsService;
        _templateRenderer = templateRenderer;
        _logger = logger;
    }

    /// <summary>
    /// Processes the notification sending request through complete workflow from template retrieval to delivery.
    /// Logic flow: 1) Retrieves and validates template (exists, active), 2) Checks user preferences for channel opt-out,
    /// 3) Renders template subject and body with placeholder data, 4) Creates notification log with Pending status,
    /// 5) Dispatches to Email or SMS service based on channel, 6) Updates log status to Sent/Failed with timestamp/error,
    /// 7) Returns notification log ID for tracking. Respects user preferences by skipping delivery if channel disabled.
    /// Implements graceful error handling by catching delivery exceptions and recording them in log for retry processing.
    /// </summary>
    /// <param name="request">Send notification command containing user ID, recipient contact info, channel, template ID, placeholder data, and event context.</param>
    /// <param name="cancellationToken">Cancellation token for aborting long-running operations.</param>
    /// <returns>
    /// The unique identifier of the created notification log entry for tracking delivery status.
    /// Returns Guid.Empty if notification was skipped due to user preference opt-out (channel disabled).
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when template not found or template is inactive.</exception>
    public async Task<Guid> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        // Get template - validates template exists and retrieves subject, body, and required placeholders
        var template = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template == null)
        {
            throw new InvalidOperationException($"Template with ID {request.TemplateId} not found");
        }

        // Validate template is active - inactive templates cannot be used for new notifications
        if (!template.IsActive)
        {
            throw new InvalidOperationException($"Template {template.Name} is not active");
        }

        // Check user preferences - respects user's channel opt-out decisions
        var preference = await _preferenceRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (preference != null)
        {
            // Check if Email channel is enabled - skip if user opted out of emails
            if (request.Channel == NotificationChannel.Email && !preference.EmailEnabled)
            {
                _logger.LogInformation("Email notifications disabled for user {UserId}", request.UserId);
                return Guid.Empty; // Return empty GUID to indicate notification was skipped
            }
            // Check if SMS channel is enabled - skip if user opted out of SMS
            if (request.Channel == NotificationChannel.SMS && !preference.SmsEnabled)
            {
                _logger.LogInformation("SMS notifications disabled for user {UserId}", request.UserId);
                return Guid.Empty; // Return empty GUID to indicate notification was skipped
            }
        }

        // Render template - replaces {{placeholders}} with actual values from PlaceholderData dictionary
        var subject = await _templateRenderer.RenderAsync(template.Subject, request.PlaceholderData, cancellationToken);
        var body = await _templateRenderer.RenderAsync(template.BodyTemplate, request.PlaceholderData, cancellationToken);

        // Create notification log - records notification attempt for audit trail and retry processing
        var notificationLog = new NotificationLog
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            RecipientEmail = request.RecipientEmail,
            RecipientPhone = request.RecipientPhone,
            Channel = request.Channel,
            TemplateId = request.TemplateId,
            Subject = subject,
            Body = body,
            Status = NotificationStatus.Pending, // Initial status before delivery attempt
            RetryCount = 0, // Tracks number of retry attempts for failed deliveries
            EventType = request.EventType, // Business event that triggered notification
            EventData = request.EventData, // Full event payload for debugging
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.UserId
        };

        // Persist log with Pending status - allows tracking even if delivery fails
        await _notificationLogRepository.AddAsync(notificationLog, cancellationToken);

        // Send notification through appropriate channel service
        bool success = false;
        try
        {
            // Dispatch to Email service - uses SMTP to send HTML/plain text email
            if (request.Channel == NotificationChannel.Email)
            {
                success = await _emailService.SendEmailAsync(request.RecipientEmail, subject, body, cancellationToken);
            }
            // Dispatch to SMS service - uses Twilio API to send text message
            else if (request.Channel == NotificationChannel.SMS)
            {
                success = await _smsService.SendSmsAsync(request.RecipientPhone, body, cancellationToken);
            }

            // Update log status based on delivery result
            if (success)
            {
                notificationLog.Status = NotificationStatus.Sent;
                notificationLog.SentAt = DateTime.UtcNow; // Record successful delivery timestamp
            }
            else
            {
                notificationLog.Status = NotificationStatus.Failed;
                notificationLog.ErrorMessage = "Failed to send notification"; // Generic failure message
            }
        }
        catch (Exception ex)
        {
            // Catch delivery exceptions - network errors, API failures, invalid recipients, etc.
            _logger.LogError(ex, "Error sending notification to user {UserId}", request.UserId);
            notificationLog.Status = NotificationStatus.Failed;
            notificationLog.ErrorMessage = ex.Message; // Store exception message for debugging
        }

        // Update log with final status - allows retry processing to identify failed notifications
        await _notificationLogRepository.UpdateAsync(notificationLog, cancellationToken);

        return notificationLog.Id; // Return log ID for tracking and status queries
    }
}
