using Microsoft.EntityFrameworkCore;
using Ship24X7.Notification.Application.Interfaces;
using Ship24X7.Notification.Domain.Entities;
using Ship24X7.Notification.Infrastructure.Persistence;

namespace Ship24X7.Notification.Infrastructure.Repositories;

/// <summary>
/// Repository for managing NotificationLog persistence operations. Handles database CRUD operations and queries.
/// </summary>
public class NotificationLogRepository : INotificationLogRepository
{
    private readonly NotificationDbContext _context;

    public NotificationLogRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationLog> AddAsync(NotificationLog notificationLog, CancellationToken cancellationToken = default)
    {
        await _context.NotificationLogs.AddAsync(notificationLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return notificationLog;
    }

    public async Task<NotificationLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationLogs
            .Include(n => n.Template)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<List<NotificationLog>> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationLogs
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(NotificationLog notificationLog, CancellationToken cancellationToken = default)
    {
        _context.NotificationLogs.Update(notificationLog);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
