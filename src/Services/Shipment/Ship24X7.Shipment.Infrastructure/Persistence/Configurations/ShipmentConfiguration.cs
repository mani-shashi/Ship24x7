using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ship24X7.Shipment.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Shipment entity. Defines table structure, relationships, and constraints.
/// </summary>
public class ShipmentConfiguration : IEntityTypeConfiguration<Domain.Entities.Shipment>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Shipment> builder)
    {
        builder.ToTable("Shipments");
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.TrackingNumber)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(s => s.TrackingNumber)
            .IsUnique();
        
        builder.Property(s => s.IdempotencyKey)
            .HasMaxLength(100);
        
        builder.HasIndex(s => s.IdempotencyKey);
        
        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(s => s.Currency)
            .IsRequired()
            .HasMaxLength(3);
        
        builder.Property(s => s.TotalCost)
            .HasPrecision(18, 2);
        
        builder.Property(s => s.BaseRate)
            .HasPrecision(18, 2);
        
        builder.Property(s => s.FuelSurcharge)
            .HasPrecision(18, 2);
        
        builder.Property(s => s.InsuranceCost)
            .HasPrecision(18, 2);
        
        builder.Property(s => s.ActualWeight)
            .HasPrecision(18, 2);
        
        builder.Property(s => s.VolumetricWeight)
            .HasPrecision(18, 2);
        
        builder.Property(s => s.ChargeableWeight)
            .HasPrecision(18, 2);
        
        builder.Property(s => s.DeclaredValue)
            .HasPrecision(18, 2);
        
        // Relationships
        builder.HasOne(s => s.SenderAddress)
            .WithMany()
            .HasForeignKey(s => s.SenderAddressId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(s => s.ReceiverAddress)
            .WithMany()
            .HasForeignKey(s => s.ReceiverAddressId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(s => s.ServiceRate)
            .WithMany()
            .HasForeignKey(s => s.ServiceRateId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(s => s.OriginHub)
            .WithMany()
            .HasForeignKey(s => s.OriginHubId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(s => s.DestinationHub)
            .WithMany()
            .HasForeignKey(s => s.DestinationHubId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.CurrentHub)
            .WithMany()
            .HasForeignKey(s => s.CurrentHubId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(s => s.DeliveryOtpHash)
            .HasMaxLength(64);   // SHA-256 hex = 64 chars

        builder.Property(s => s.DeliveryAgentId)
            .HasMaxLength(100);
        
        builder.HasMany(s => s.Items)
            .WithOne(i => i.Shipment)
            .HasForeignKey(i => i.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(s => s.Pickup)
            .WithOne(p => p.Shipment)
            .HasForeignKey<Domain.Entities.Pickup>(p => p.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
