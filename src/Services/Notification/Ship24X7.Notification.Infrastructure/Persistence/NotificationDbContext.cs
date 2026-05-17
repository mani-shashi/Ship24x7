using Microsoft.EntityFrameworkCore;
using Ship24X7.Notification.Domain.Entities;
using Ship24X7.Notification.Infrastructure.Persistence.Configurations;

namespace Ship24X7.Notification.Infrastructure.Persistence;

/// <summary>
/// Database context for Notification service. Manages entity sets and database operations.
/// </summary>
public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new NotificationLogConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationPreferenceConfiguration());
    }
}
