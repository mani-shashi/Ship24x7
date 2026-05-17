using Ship24X7.Shared.Domain;
using Ship24X7.Notification.Domain.ValueObjects;

namespace Ship24X7.Notification.Domain.Events;

/// <summary>
/// Domain event raised when NotificationFailed occurs. Used for event-driven architecture and integration.
/// </summary>
public class NotificationFailed : BaseDomainEvent
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
    /// Gets or sets the Error message.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Retry count.
    /// </summary>
    public int RetryCount { get; set; }
    /// <summary>
    /// Gets or sets the failedat.
    /// </summary>
    public DateTime FailedAt { get; set; }
}
