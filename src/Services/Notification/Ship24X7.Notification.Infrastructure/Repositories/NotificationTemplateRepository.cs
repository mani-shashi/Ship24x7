using Microsoft.EntityFrameworkCore;
using Ship24X7.Notification.Application.Interfaces;
using Ship24X7.Notification.Domain.Entities;
using Ship24X7.Notification.Infrastructure.Persistence;

namespace Ship24X7.Notification.Infrastructure.Repositories;

/// <summary>
/// Repository for managing NotificationTemplate persistence operations. Handles database CRUD operations and queries.
/// </summary>
public class NotificationTemplateRepository : INotificationTemplateRepository
{
    private readonly NotificationDbContext _context;

    public NotificationTemplateRepository(NotificationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationTemplate> AddAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        await _context.NotificationTemplates.AddAsync(template, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return template;
    }

    public async Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<List<NotificationTemplate>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = _context.NotificationTemplates.AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(t => t.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(NotificationTemplate template, CancellationToken cancellationToken = default)
    {
        _context.NotificationTemplates.Update(template);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await _context.NotificationTemplates.FindAsync(new object[] { id }, cancellationToken);
        if (template != null)
        {
            template.IsDeleted = true;
            template.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
