namespace Ship24X7.Notification.Domain.ValueObjects;

/// <summary>
/// Value object representing NotificationChannel in the domain model. Immutable type with value-based equality.
/// </summary>
public enum NotificationChannel
{
    Email,
    SMS,
    Push,
    InApp
}
