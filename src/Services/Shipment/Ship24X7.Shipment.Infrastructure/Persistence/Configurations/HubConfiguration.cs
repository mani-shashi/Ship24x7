using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Shipment.Domain.Entities;

namespace Ship24X7.Shipment.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Hub entity. Defines table structure, relationships, and constraints.
/// </summary>
public class HubConfiguration : IEntityTypeConfiguration<Hub>
{
    public void Configure(EntityTypeBuilder<Hub> builder)
    {
        builder.ToTable("Hubs");
        
        builder.HasKey(h => h.Id);
        
        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(h => h.Code)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.HasIndex(h => h.Code)
            .IsUnique();
        
        builder.Property(h => h.AddressLine1)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(h => h.AddressLine2)
            .HasMaxLength(500);
        
        builder.Property(h => h.City)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(h => h.State)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(h => h.PostalCode)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.Property(h => h.Country)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(h => h.Latitude)
            .HasPrecision(10, 6);
        
        builder.Property(h => h.Longitude)
            .HasPrecision(10, 6);
    }
}
