using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Shipment.Domain.Entities;

namespace Ship24X7.Shipment.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for ShipmentStatusHistory.
/// Append-only table — no updates or deletes are expected.
/// </summary>
public class ShipmentStatusHistoryConfiguration : IEntityTypeConfiguration<ShipmentStatusHistory>
{
    public void Configure(EntityTypeBuilder<ShipmentStatusHistory> builder)
    {
        builder.ToTable("ShipmentStatusHistory");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.FromStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(h => h.ToStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(h => h.Reason)
            .HasMaxLength(1000);

        builder.Property(h => h.ChangedAt)
            .IsRequired();

        // Index for fast per-shipment history queries
        builder.HasIndex(h => h.ShipmentId);

        // Index for auditing by operator
        builder.HasIndex(h => h.ChangedBy);

        builder.HasOne(h => h.Shipment)
            .WithMany(s => s.StatusHistory)
            .HasForeignKey(h => h.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
