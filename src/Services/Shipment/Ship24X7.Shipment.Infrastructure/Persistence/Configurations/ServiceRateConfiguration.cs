using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Shipment.Domain.Entities;

namespace Ship24X7.Shipment.Infrastructure.Persistence.Configurations;

/// <summary>
/// RateConfiguration service implementation. Provides rateconfiguration functionality for the application.
/// </summary>
public class ServiceRateConfiguration : IEntityTypeConfiguration<ServiceRate>
{
    public void Configure(EntityTypeBuilder<ServiceRate> builder)
    {
        builder.ToTable("ServiceRates");
        
        builder.HasKey(sr => sr.Id);
        
        builder.Property(sr => sr.ServiceType)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(sr => sr.ServiceName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(sr => sr.BaseRatePerKg)
            .HasPrecision(18, 2);
        
        builder.Property(sr => sr.FuelSurchargePercent)
            .HasPrecision(5, 2);
        
        builder.Property(sr => sr.MinimumCharge)
            .HasPrecision(18, 2);
    }
}
