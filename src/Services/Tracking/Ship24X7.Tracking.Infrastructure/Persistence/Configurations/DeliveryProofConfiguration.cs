using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Tracking.Domain.Entities;

namespace Ship24X7.Tracking.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for DeliveryProof entity. Defines table structure, relationships, and constraints.
/// </summary>
public class DeliveryProofConfiguration : IEntityTypeConfiguration<DeliveryProof>
{
    public void Configure(EntityTypeBuilder<DeliveryProof> builder)
    {
        builder.ToTable("DeliveryProofs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ShipmentId)
            .IsRequired();

        builder.Property(e => e.TrackingNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.ReceivedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.DeliveryDate)
            .IsRequired();

        builder.Property(e => e.SignatureImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.PhotoProofUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Latitude)
            .IsRequired()
            .HasPrecision(10, 6);

        builder.Property(e => e.Longitude)
            .IsRequired()
            .HasPrecision(10, 6);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.DeliveredBy)
            .IsRequired();

        builder.HasIndex(e => e.ShipmentId)
            .IsUnique();
        builder.HasIndex(e => e.TrackingNumber);
    }
}
