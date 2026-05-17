using MediatR;
using Ship24X7.Notification.Domain.ValueObjects;

namespace Ship24X7.Notification.Application.Commands;

/// <summary>
/// Represents a command to send a notification to a user through a specified channel (Email/SMS/Push).
/// Encapsulates all data required for notification delivery including recipient information, template selection, and dynamic placeholder data.
/// Used by event consumers (ShipmentEventConsumer, PaymentEventConsumer) to trigger notifications based on business events.
/// Implements CQRS pattern by returning the notification log ID for tracking and audit purposes.
/// </summary>
public class SendNotificationCommand : IRequest<Guid>
{
    /// <summary>
    /// Gets or sets the unique identifier of the user receiving the notification.
    /// Used to check user preferences and determine if notification should be sent based on channel enablement and quiet hours.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the recipient's email address for Email channel notifications.
    /// Required when Channel is Email. Must be valid email format. Used as destination for SMTP delivery.
    /// </summary>
    public string RecipientEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the recipient's phone number for SMS channel notifications.
    /// Required when Channel is SMS. Must be in E.164 format (+country code). Used as destination for Twilio SMS delivery.
    /// </summary>
    public string RecipientPhone { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the notification delivery channel (Email, SMS, or Push).
    /// Determines which service (SmtpEmailService, TwilioSmsService) will be used for delivery.
    /// Handler checks user preferences to ensure channel is enabled before sending.
    /// </summary>
    public NotificationChannel Channel { get; set; }
    
    /// <summary>
    /// Gets or sets the unique identifier of the notification template to use.
    /// Template contains subject, body with {{placeholders}}, and required placeholder names.
    /// Handler retrieves template from database and renders it with PlaceholderData before sending.
    /// </summary>
    public Guid TemplateId { get; set; }
    
    /// <summary>
    /// Gets or sets the dictionary of placeholder names and their replacement values.
    /// Keys must match template's RequiredPlaceholders array. Values are injected into template body during rendering.
    /// Example: {"CustomerName": "John Doe", "TrackingNumber": "TRK123456", "DeliveryDate": "2024-01-15"}.
    /// </summary>
    public Dictionary<string, string> PlaceholderData { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the business event type that triggered this notification.
    /// Examples: "ShipmentBooked", "PaymentCaptured", "DeliveryCompleted". Used for filtering and analytics.
    /// Stored in NotificationLog for audit trail and debugging event-driven notification flows.
    /// </summary>
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the serialized JSON data of the business event that triggered this notification.
    /// Contains full event payload for debugging and audit purposes. Stored in NotificationLog.
    /// Allows reconstruction of notification context if resend or investigation is needed.
    /// </summary>
    public string EventData { get; set; } = string.Empty;
}
