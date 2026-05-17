using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Tracking.Domain.Entities;

namespace Ship24X7.Tracking.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for TrackingEvent entity. Defines table structure, relationships, and constraints.
/// </summary>
public class TrackingEventConfiguration : IEntityTypeConfiguration<TrackingEvent>
{
    public void Configure(EntityTypeBuilder<TrackingEvent> builder)
    {
        builder.ToTable("TrackingEvents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ShipmentId)
            .IsRequired();

        builder.Property(e => e.TrackingNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(100);

        builder.Property(e => e.RawStatus)
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Location)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.EventTimestamp)
            .IsRequired();

        builder.Property(e => e.IsException)
            .IsRequired();

        builder.Property(e => e.ExceptionReason)
            .HasMaxLength(500);

        builder.Property(e => e.RecordedBy)
            .HasMaxLength(100);

        builder.HasIndex(e => e.ShipmentId);
        builder.HasIndex(e => e.TrackingNumber);
        builder.HasIndex(e => e.EventTimestamp);
    }
}
