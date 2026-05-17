using Ship24X7.Shared.Domain;
using Ship24X7.Notification.Domain.ValueObjects;

namespace Ship24X7.Notification.Domain.Entities;

/// <summary>
/// Represents a notification delivery log entry in the Ship24X7 platform.
/// Tracks the complete lifecycle of a notification from creation through delivery attempt to final status.
/// Stores rendered content (subject/body after placeholder replacement), delivery metadata (channel, recipient, timestamp),
/// status tracking (Pending/Sent/Failed), retry information, and event context for audit trail and debugging.
/// Used for notification history queries, retry processing of failed deliveries, and analytics on notification effectiveness.
/// Inherits audit fields (CreatedAt, CreatedBy, CorrelationId) from BaseEntity for distributed tracing.
/// </summary>
public class NotificationLog : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this notification log entry.
    /// Primary key used for querying notification history and tracking delivery status.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the unique identifier of the user who received or should receive this notification.
    /// Foreign key to User entity in Auth service. Used to query notification history per user.
    /// Also used to check user preferences before delivery to respect channel opt-outs.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the recipient's email address where the notification was sent (Email channel only).
    /// Populated when Channel is Email. Stored for audit trail even if delivery fails.
    /// May differ from user's current email if user changed email after notification was queued.
    /// </summary>
    public string RecipientEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the recipient's phone number where the notification was sent (SMS channel only).
    /// Populated when Channel is SMS. Stored in E.164 format (+country code).
    /// May differ from user's current phone if user changed phone after notification was queued.
    /// </summary>
    public string RecipientPhone { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the delivery channel used for this notification (Email, SMS, or Push).
    /// Determines which service (SmtpEmailService, TwilioSmsService) was used for delivery.
    /// Used for filtering notification history by channel and analyzing channel effectiveness.
    /// </summary>
    public NotificationChannel Channel { get; set; }
    
    /// <summary>
    /// Gets or sets the unique identifier of the template used to generate this notification.
    /// Foreign key to NotificationTemplate entity. Allows querying which notifications used which template.
    /// Template may have been modified or deactivated after this notification was sent.
    /// </summary>
    public Guid TemplateId { get; set; }
    
    /// <summary>
    /// Gets or sets the rendered subject line after placeholder replacement (Email channel only).
    /// Contains final subject text sent to recipient with all {{placeholders}} replaced with actual values.
    /// Empty for SMS and Push channels which don't use subjects.
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the rendered message body after placeholder replacement.
    /// Contains final content sent to recipient with all {{placeholders}} replaced with actual values.
    /// For Email: HTML or plain text. For SMS: plain text under 160 chars. For Push: short text.
    /// </summary>
    public string Body { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the current delivery status of this notification.
    /// Values: Pending (queued but not sent), Sent (successfully delivered), Failed (delivery error).
    /// Used by retry processing to identify failed notifications that need redelivery attempts.
    /// </summary>
    public NotificationStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets the number of delivery retry attempts made for this notification.
    /// Incremented each time retry processing attempts redelivery after failure.
    /// Used to implement exponential backoff and maximum retry limits (typically 3 attempts).
    /// </summary>
    public int RetryCount { get; set; }
    
    /// <summary>
    /// Gets or sets the UTC timestamp when the notification was successfully delivered.
    /// Null if notification is still Pending or Failed. Set when Status changes to Sent.
    /// Used for delivery time analytics and SLA tracking (time from creation to delivery).
    /// </summary>
    public DateTime? SentAt { get; set; }
    
    /// <summary>
    /// Gets or sets the error message if notification delivery failed.
    /// Contains exception message or service error response for debugging failed deliveries.
    /// Examples: "SMTP connection timeout", "Invalid phone number", "Twilio API rate limit exceeded".
    /// Null if notification was sent successfully or is still pending.
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gets or sets the business event type that triggered this notification.
    /// Examples: "ShipmentBooked", "PaymentCaptured", "ShipmentDelivered", "ShipmentDelayed".
    /// Used for filtering notifications by event type and analyzing which events generate most notifications.
    /// </summary>
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the serialized JSON data of the business event that triggered this notification.
    /// Contains full event payload (shipment details, payment info, etc.) for debugging and audit.
    /// Allows reconstruction of notification context if resend is needed or investigation required.
    /// </summary>
    public string EventData { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the navigation property to the template used for this notification.
    /// Lazy-loaded relationship to NotificationTemplate entity. Allows querying template details from log.
    /// May be null if template was deleted after notification was sent.
    /// </summary>
    public NotificationTemplate? Template { get; set; }
}
