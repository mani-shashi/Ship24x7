using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for MfaSettings entity.
/// Defines two-factor authentication settings with TOTP secret and backup codes storage.
/// </summary>
public class MfaSettingsConfiguration : IEntityTypeConfiguration<MfaSettings>
{
    /// <summary>
    /// Configures the MfaSettings entity mapping including backup codes conversion to comma-separated string.
    /// Stores TOTP secret and backup codes for multi-factor authentication.
    /// </summary>
    /// <param name="builder">Entity type builder for MfaSettings configuration.</param>
    public void Configure(EntityTypeBuilder<MfaSettings> builder)
    {
        builder.ToTable("MfaSettings");

        builder.HasKey(mfa => mfa.Id);

        builder.Property(mfa => mfa.TotpSecret)
            .HasMaxLength(256);

        builder.Property(mfa => mfa.BackupCodes)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .HasMaxLength(1000);
    }
}
