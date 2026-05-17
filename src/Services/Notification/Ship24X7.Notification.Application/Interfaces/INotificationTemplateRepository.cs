using Ship24X7.Notification.Domain.Entities;

namespace Ship24X7.Notification.Application.Interfaces;

/// <summary>
/// Repository for managing INotificationTemplate persistence operations. Handles database CRUD operations and queries.
/// </summary>
public interface INotificationTemplateRepository
{
    Task<NotificationTemplate> AddAsync(NotificationTemplate template, CancellationToken cancellationToken = default);
    Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<NotificationTemplate>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(NotificationTemplate template, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
