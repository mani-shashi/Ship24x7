using Ship24X7.Shared.Domain;
using Ship24X7.Notification.Domain.ValueObjects;

namespace Ship24X7.Notification.Domain.Events;

/// <summary>
/// Domain event raised when NotificationSent occurs. Used for event-driven architecture and integration.
/// </summary>
public class NotificationSent : BaseDomainEvent
{
    /// <summary>
    /// Gets or sets the notificationLogId.
    /// </summary>
    public Guid NotificationLogId { get; set; }
    /// <summary>
    /// Gets or sets the userId.
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// Gets or sets the channel.
    /// </summary>
    public NotificationChannel Channel { get; set; }
    /// <summary>
    /// Gets or sets the Recipient email.
    /// </summary>
    public string RecipientEmail { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Recipient phone.
    /// </summary>
    public string RecipientPhone { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the subject line.
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the sentat.
    /// </summary>
    public DateTime SentAt { get; set; }
}
