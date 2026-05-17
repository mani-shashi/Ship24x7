using Ship24X7.Notification.Domain.Entities;

namespace Ship24X7.Notification.Application.Interfaces;

/// <summary>
/// Repository for managing INotificationPreference persistence operations. Handles database CRUD operations and queries.
/// </summary>
public interface INotificationPreferenceRepository
{
    Task<NotificationPreference> AddAsync(NotificationPreference preference, CancellationToken cancellationToken = default);
    Task<NotificationPreference?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(NotificationPreference preference, CancellationToken cancellationToken = default);
}
