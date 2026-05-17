using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for UserAddress entity.
/// </summary>
public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("UserAddresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Label)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.ContactName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.ContactPhone)
            .HasMaxLength(20);

        builder.Property(a => a.AddressLine1)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.AddressLine2)
            .HasMaxLength(200);

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
            .HasMaxLength(10)
            .HasDefaultValue("IN");

        builder.Property(a => a.Type)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Other");
    }
}
