using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for ExternalLogin entity.
/// Defines OAuth provider associations with unique constraint on provider and key combination.
/// </summary>
public class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
    /// <summary>
    /// Configures the ExternalLogin entity mapping including composite unique index on provider and key.
    /// Stores OAuth tokens and provider information for third-party authentication.
    /// </summary>
    /// <param name="builder">Entity type builder for ExternalLogin configuration.</param>
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        builder.ToTable("ExternalLogins");

        builder.HasKey(el => el.Id);

        builder.Property(el => el.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(el => el.ProviderKey)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(el => new { el.Provider, el.ProviderKey })
            .IsUnique();

        builder.Property(el => el.ProviderDisplayName)
            .HasMaxLength(100);

        builder.Property(el => el.AccessToken)
            .HasMaxLength(1000);

        builder.Property(el => el.RefreshToken)
            .HasMaxLength(1000);
    }
}
