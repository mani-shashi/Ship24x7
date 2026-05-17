using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Payment.Domain.Entities;

namespace Ship24X7.Payment.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for PaymentRefund entity. Defines table structure, relationships, and constraints.
/// </summary>
public class PaymentRefundConfiguration : IEntityTypeConfiguration<PaymentRefund>
{
    public void Configure(EntityTypeBuilder<PaymentRefund> builder)
    {
        builder.ToTable("PaymentRefunds");

        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.PaymentOrderId)
            .IsRequired();

        builder.HasIndex(pr => pr.PaymentOrderId);

        builder.Property(pr => pr.RazorpayRefundId)
            .HasMaxLength(100);

        builder.HasIndex(pr => pr.RazorpayRefundId);

        builder.Property(pr => pr.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(pr => pr.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(pr => pr.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(pr => pr.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(pr => pr.InitiatedBy)
            .IsRequired();
    }
}
