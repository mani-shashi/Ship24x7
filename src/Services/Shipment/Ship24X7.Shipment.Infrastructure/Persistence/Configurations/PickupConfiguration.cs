using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Shipment.Domain.Entities;

namespace Ship24X7.Shipment.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Pickup entity. Defines table structure, relationships, and constraints.
/// </summary>
public class PickupConfiguration : IEntityTypeConfiguration<Pickup>
{
    public void Configure(EntityTypeBuilder<Pickup> builder)
    {
        builder.ToTable("Pickups");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.ConfirmationNumber)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(p => p.ConfirmationNumber)
            .IsUnique();
        
        builder.Property(p => p.TimeSlot)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(p => p.Notes)
            .HasMaxLength(1000);
    }
}
