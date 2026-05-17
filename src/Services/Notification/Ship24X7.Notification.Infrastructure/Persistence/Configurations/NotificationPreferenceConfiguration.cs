using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Notification.Domain.Entities;

namespace Ship24X7.Notification.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for NotificationPreference entity. Defines table structure, relationships, and constraints.
/// </summary>
public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("NotificationPreferences");

        builder.HasKey(p => p.Id);

        builder.HasIndex(p => p.UserId)
            .IsUnique();
    }
}
