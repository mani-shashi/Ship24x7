using Ship24X7.Notification.Domain.Entities;

namespace Ship24X7.Notification.Application.Interfaces;

/// <summary>
/// Repository for managing INotificationLog persistence operations. Handles database CRUD operations and queries.
/// </summary>
public interface INotificationLogRepository
{
    Task<NotificationLog> AddAsync(NotificationLog notificationLog, CancellationToken cancellationToken = default);
    Task<NotificationLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<NotificationLog>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task UpdateAsync(NotificationLog notificationLog, CancellationToken cancellationToken = default);
}
