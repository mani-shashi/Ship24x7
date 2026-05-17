using Microsoft.EntityFrameworkCore;
using Ship24X7.Shipment.Domain.Entities;
using Ship24X7.Shipment.Infrastructure.Persistence.Configurations;

namespace Ship24X7.Shipment.Infrastructure.Persistence;

/// <summary>
/// Database context for Shipment service. Manages entity sets and database operations.
/// </summary>
public class ShipmentDbContext : DbContext
{
    public ShipmentDbContext(DbContextOptions<ShipmentDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.Shipment> Shipments => Set<Domain.Entities.Shipment>();
    public DbSet<ShipmentItem> ShipmentItems => Set<ShipmentItem>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Pickup> Pickups => Set<Pickup>();
    public DbSet<ServiceRate> ServiceRates => Set<ServiceRate>();
    public DbSet<Hub> Hubs => Set<Hub>();
    public DbSet<ShipmentStatusHistory> ShipmentStatusHistory => Set<ShipmentStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ShipmentConfiguration());
        modelBuilder.ApplyConfiguration(new ShipmentItemConfiguration());
        modelBuilder.ApplyConfiguration(new AddressConfiguration());
        modelBuilder.ApplyConfiguration(new PickupConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceRateConfiguration());
        modelBuilder.ApplyConfiguration(new HubConfiguration());
        modelBuilder.ApplyConfiguration(new ShipmentStatusHistoryConfiguration());
    }
}
