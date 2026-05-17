using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for UserClaim entity.
/// Defines custom claims and permissions that can be assigned to users.
/// </summary>
public class UserClaimConfiguration : IEntityTypeConfiguration<UserClaim>
{
    /// <summary>
    /// Configures the UserClaim entity mapping including claim type and value constraints.
    /// </summary>
    /// <param name="builder">Entity type builder for UserClaim configuration.</param>
    public void Configure(EntityTypeBuilder<UserClaim> builder)
    {
        builder.ToTable("UserClaims");

        builder.HasKey(uc => uc.Id);

        builder.Property(uc => uc.ClaimType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(uc => uc.ClaimValue)
            .IsRequired()
            .HasMaxLength(500);
    }
}
