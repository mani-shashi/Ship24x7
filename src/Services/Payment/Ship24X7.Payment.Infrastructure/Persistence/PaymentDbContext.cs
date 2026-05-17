using Microsoft.EntityFrameworkCore;
using Ship24X7.Payment.Domain.Entities;
using Ship24X7.Payment.Infrastructure.Persistence.Configurations;

namespace Ship24X7.Payment.Infrastructure.Persistence;

/// <summary>
/// Database context for Payment service. Manages entity sets and database operations.
/// </summary>
public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    public DbSet<PaymentOrder> PaymentOrders => Set<PaymentOrder>();
    public DbSet<PaymentRefund> PaymentRefunds => Set<PaymentRefund>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new PaymentOrderConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentRefundConfiguration());
    }
}
