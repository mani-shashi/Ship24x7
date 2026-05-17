using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for VerificationToken entity.
/// Defines tokens for email verification, phone verification, and password reset operations.
/// </summary>
public class VerificationTokenConfiguration : IEntityTypeConfiguration<VerificationToken>
{
    /// <summary>
    /// Configures the VerificationToken entity mapping including token index and type enum conversion.
    /// Supports multiple verification types with expiration tracking.
    /// </summary>
    /// <param name="builder">Entity type builder for VerificationToken configuration.</param>
    public void Configure(EntityTypeBuilder<VerificationToken> builder)
    {
        builder.ToTable("VerificationTokens");

        builder.HasKey(vt => vt.Id);

        builder.Property(vt => vt.Token)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(vt => vt.Token);

        builder.Property(vt => vt.Type)
            .IsRequired()
            .HasConversion<string>();
    }
}
