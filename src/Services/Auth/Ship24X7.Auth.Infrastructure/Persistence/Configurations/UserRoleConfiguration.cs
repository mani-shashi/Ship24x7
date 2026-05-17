using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for UserRole entity.
/// Defines many-to-many relationship between users and roles with unique constraint.
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    /// <summary>
    /// Configures the UserRole entity mapping including composite unique index on UserId and RoleId.
    /// Prevents duplicate role assignments to the same user.
    /// </summary>
    /// <param name="builder">Entity type builder for UserRole configuration.</param>
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

        builder.HasKey(ur => ur.Id);

        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique();
    }
}
