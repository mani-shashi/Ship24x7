using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for User entity.
/// Defines table structure, indexes, constraints, and relationships with related entities.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// Configures the User entity mapping including properties, indexes, and relationships.
    /// Sets up cascade delete for refresh tokens, external logins, MFA settings, verification tokens, roles, and claims.
    /// </summary>
    /// <param name="builder">Entity type builder for User configuration.</param>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(512);

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ExternalLogins)
            .WithOne(el => el.User)
            .HasForeignKey(el => el.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.MfaSettings)
            .WithOne(mfa => mfa.User)
            .HasForeignKey<MfaSettings>(mfa => mfa.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.VerificationTokens)
            .WithOne(vt => vt.User)
            .HasForeignKey(vt => vt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.UserClaims)
            .WithOne(uc => uc.User)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Addresses)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Preferences)
            .WithOne(p => p.User)
            .HasForeignKey<UserPreferences>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(u => u.ProfilePhotoUrl)
            .HasMaxLength(500);
    }
}
