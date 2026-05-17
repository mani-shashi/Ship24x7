using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Notification.Domain.Entities;

namespace Ship24X7.Notification.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for NotificationTemplate entity. Defines table structure, relationships, and constraints.
/// </summary>
public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("NotificationTemplates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.BodyTemplate)
            .IsRequired();

        builder.Property(t => t.RequiredPlaceholders)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .HasMaxLength(1000);

        builder.HasIndex(t => t.Name);
        builder.HasIndex(t => t.Type);
        builder.HasIndex(t => t.IsActive);

        builder.HasMany(t => t.NotificationLogs)
            .WithOne(n => n.Template)
            .HasForeignKey(n => n.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
