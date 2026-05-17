using Ship24X7.Notification.Domain.ValueObjects;

namespace Ship24X7.Notification.Application.DTOs;

/// <summary>
/// Data transfer object for NotificationTemplate data. Used for API responses and data serialization.
/// </summary>
public class NotificationTemplateResponse
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    public TemplateType Type { get; set; }
    /// <summary>
    /// Gets or sets the channel.
    /// </summary>
    public NotificationChannel Channel { get; set; }
    /// <summary>
    /// Gets or sets the subject line.
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Body template.
    /// </summary>
    public string BodyTemplate { get; set; } = string.Empty;
    public string[] RequiredPlaceholders { get; set; } = Array.Empty<string>();
    /// <summary>
    /// Gets or sets a value indicating whether this entity is active.
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
