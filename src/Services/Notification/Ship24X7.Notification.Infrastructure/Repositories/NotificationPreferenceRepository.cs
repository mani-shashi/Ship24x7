using Microsoft.EntityFrameworkCore;
using Ship24X7.Notification.Application.Interfaces;
using Ship24X7.Notification.Domain.Entities;
using Ship24X7.Notification.Infrastructure.Persistence;

namespace Ship24X7.Notification.Infrastructure.Repositories;

/// <summary>
/// Repository for managing NotificationPreference persistence operations. Handles database CRUD operations and queries.
/// </summary>
public class NotificationPreferenceRepository : INotificationPreferenceRepository
{
    private readonly NotificationDbContext _context;

    public NotificationPreferenceRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationPreference> AddAsync(NotificationPreference preference, CancellationToken cancellationToken = default)
    {
        await _context.NotificationPreferences.AddAsync(preference, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return preference;
    }

    public async Task<NotificationPreference?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task UpdateAsync(NotificationPreference preference, CancellationToken cancellationToken = default)
    {
        _context.NotificationPreferences.Update(preference);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
