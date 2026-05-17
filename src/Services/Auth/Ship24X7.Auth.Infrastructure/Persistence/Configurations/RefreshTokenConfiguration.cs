using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for RefreshToken entity.
/// Defines token storage with unique constraint and token family tracking for rotation.
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    /// <summary>
    /// Configures the RefreshToken entity mapping including unique token constraint and family tracking.
    /// Supports token rotation security pattern with replacement tracking.
    /// </summary>
    /// <param name="builder">Entity type builder for RefreshToken configuration.</param>
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.HasIndex(rt => rt.Token)
            .IsUnique();

        builder.Property(rt => rt.TokenFamily)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rt => rt.ReplacedByToken)
            .HasMaxLength(512);
    }
}
