using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Notification.Domain.Entities;

namespace Ship24X7.Notification.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for NotificationLog entity. Defines table structure, relationships, and constraints.
/// </summary>
public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.ToTable("NotificationLogs");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.RecipientEmail)
            .HasMaxLength(256);

        builder.Property(n => n.RecipientPhone)
            .HasMaxLength(20);

        builder.Property(n => n.Subject)
            .HasMaxLength(500);

        builder.Property(n => n.Body)
            .IsRequired();

        builder.Property(n => n.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(n => n.EventType)
            .HasMaxLength(100);

        builder.Property(n => n.EventData)
            .HasMaxLength(4000);

        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => n.CreatedAt);

        builder.HasOne(n => n.Template)
            .WithMany(t => t.NotificationLogs)
            .HasForeignKey(n => n.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
