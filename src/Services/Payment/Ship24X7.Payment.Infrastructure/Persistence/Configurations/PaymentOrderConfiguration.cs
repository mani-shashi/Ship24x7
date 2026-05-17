using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Payment.Domain.Entities;

namespace Ship24X7.Payment.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for PaymentOrder entity. Defines table structure, relationships, and constraints.
/// </summary>
public class PaymentOrderConfiguration : IEntityTypeConfiguration<PaymentOrder>
{
    public void Configure(EntityTypeBuilder<PaymentOrder> builder)
    {
        builder.ToTable("PaymentOrders");

        builder.HasKey(po => po.Id);

        builder.Property(po => po.ShipmentId)
            .IsRequired();

        builder.HasIndex(po => po.ShipmentId);

        builder.Property(po => po.TrackingNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(po => po.TrackingNumber);

        builder.Property(po => po.RazorpayOrderId)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(po => po.RazorpayOrderId)
            .IsUnique();

        builder.Property(po => po.RazorpayPaymentId)
            .HasMaxLength(100);

        builder.Property(po => po.RazorpaySignature)
            .HasMaxLength(256);

        builder.Property(po => po.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(po => po.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(po => po.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(po => po.FailureReason)
            .HasMaxLength(500);

        builder.Property(po => po.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(po => po.IdempotencyKey)
            .IsUnique();

        builder.HasMany(po => po.Refunds)
            .WithOne(r => r.PaymentOrder)
            .HasForeignKey(r => r.PaymentOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
