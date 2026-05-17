using Ship24X7.Notification.Domain.ValueObjects;

namespace Ship24X7.Notification.Application.DTOs;

/// <summary>
/// Data transfer object for NotificationLog data. Used for API responses and data serialization.
/// </summary>
public class NotificationLogResponse
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets the userId.
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// Gets or sets the Recipient email.
    /// </summary>
    public string RecipientEmail { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Recipient phone.
    /// </summary>
    public string RecipientPhone { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the channel.
    /// </summary>
    public NotificationChannel Channel { get; set; }
    /// <summary>
    /// Gets or sets the subject line.
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the rendered message body sent to the recipient.
    /// </summary>
    public string Body { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public NotificationStatus Status { get; set; }
    /// <summary>
    /// Gets or sets the Retry count.
    /// </summary>
    public int RetryCount { get; set; }
    /// <summary>
    /// Gets or sets the sentat.
    /// </summary>
    public DateTime? SentAt { get; set; }
    /// <summary>
    /// Gets or sets the Error message.
    /// </summary>
    public string? ErrorMessage { get; set; }
    /// <summary>
    /// Gets or sets the Event type.
    /// </summary>
    public string EventType { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
