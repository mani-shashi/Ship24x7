using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Shipment.Domain.Entities;

namespace Ship24X7.Shipment.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Address entity. Defines table structure, relationships, and constraints.
/// </summary>
public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");
        
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.ContactName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(a => a.ContactPhone)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.Property(a => a.ContactEmail)
            .HasMaxLength(200);
        
        builder.Property(a => a.AddressLine1)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(a => a.AddressLine2)
            .HasMaxLength(500);
        
        builder.Property(a => a.City)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(a => a.State)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(a => a.PostalCode)
            .IsRequired()
            .HasMaxLength(20);
        
        builder.Property(a => a.Country)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(a => a.Latitude)
            .HasPrecision(10, 6);
        
        builder.Property(a => a.Longitude)
            .HasPrecision(10, 6);
    }
}
