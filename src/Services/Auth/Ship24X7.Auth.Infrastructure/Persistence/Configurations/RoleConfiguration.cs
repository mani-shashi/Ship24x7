using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Role entity.
/// Defines table structure, unique constraints, and property specifications for role-based access control.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    /// <summary>
    /// Configures the Role entity mapping including unique name constraint and property lengths.
    /// </summary>
    /// <param name="builder">Entity type builder for Role configuration.</param>
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.Name)
            .IsUnique();

        builder.Property(r => r.Description)
            .HasMaxLength(200);
    }
}
