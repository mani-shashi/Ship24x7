using Ship24X7.Shared.Domain;

namespace Ship24X7.Auth.Domain.Entities;

/// <summary>
/// Stores per-user notification and display preferences.
/// One-to-one relationship with User.
/// </summary>
public class UserPreferences : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this preferences record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user ID these preferences belong to.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets whether the user wants email notifications.
    /// </summary>
    public bool EmailNotifications { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the user wants SMS notifications.
    /// </summary>
    public bool SmsNotifications { get; set; } = false;

    /// <summary>
    /// Gets or sets whether the user wants web push notifications.
    /// </summary>
    public bool PushNotifications { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the user wants marketing/product update emails.
    /// </summary>
    public bool MarketingEmails { get; set; } = false;

    /// <summary>
    /// Gets or sets the user's preferred UI theme ("light" or "dark").
    /// </summary>
    public string Theme { get; set; } = "light";

    /// <summary>
    /// Gets or sets the user's preferred language/locale code (e.g. "en-IN").
    /// </summary>
    public string Language { get; set; } = "en-IN";

    /// <summary>
    /// Navigation property back to the owning user.
    /// </summary>
    public User User { get; set; } = null!;
}
