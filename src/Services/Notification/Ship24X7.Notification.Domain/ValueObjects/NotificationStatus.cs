namespace Ship24X7.Notification.Domain.ValueObjects;

/// <summary>
/// Value object representing NotificationStatus in the domain model. Immutable type with value-based equality.
/// </summary>
public enum NotificationStatus
{
    Pending,
    Sent,
    Failed,
    Retrying
}
