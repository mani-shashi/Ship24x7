using Microsoft.EntityFrameworkCore;
using Ship24X7.Tracking.Domain.Entities;
using Ship24X7.Tracking.Infrastructure.Persistence.Configurations;

namespace Ship24X7.Tracking.Infrastructure.Persistence;

/// <summary>
/// Database context for Tracking service. Manages entity sets and database operations.
/// </summary>
public class TrackingDbContext : DbContext
{
    public TrackingDbContext(DbContextOptions<TrackingDbContext> options) : base(options)
    {
    }

    public DbSet<TrackingEvent> TrackingEvents => Set<TrackingEvent>();
    public DbSet<DeliveryProof> DeliveryProofs => Set<DeliveryProof>();
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TrackingEventConfiguration());
        modelBuilder.ApplyConfiguration(new DeliveryProofConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentConfiguration());
    }
}
