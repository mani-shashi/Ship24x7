using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Shipment.Domain.Entities;

namespace Ship24X7.Shipment.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for ShipmentItem entity. Defines table structure, relationships, and constraints.
/// </summary>
public class ShipmentItemConfiguration : IEntityTypeConfiguration<ShipmentItem>
{
    public void Configure(EntityTypeBuilder<ShipmentItem> builder)
    {
        builder.ToTable("ShipmentItems");
        
        builder.HasKey(i => i.Id);
        
        builder.Property(i => i.Description)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(i => i.PackageType)
            .HasMaxLength(50);
        
        builder.Property(i => i.Weight)
            .HasPrecision(18, 2);
        
        builder.Property(i => i.Length)
            .HasPrecision(18, 2);
        
        builder.Property(i => i.Width)
            .HasPrecision(18, 2);
        
        builder.Property(i => i.Height)
            .HasPrecision(18, 2);
    }
}
