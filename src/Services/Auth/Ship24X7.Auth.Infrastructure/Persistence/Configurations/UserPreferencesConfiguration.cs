using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for UserPreferences entity.
/// </summary>
public class UserPreferencesConfiguration : IEntityTypeConfiguration<UserPreferences>
{
    public void Configure(EntityTypeBuilder<UserPreferences> builder)
    {
        builder.ToTable("UserPreferences");

        builder.HasKey(p => p.Id);

        builder.HasIndex(p => p.UserId)
            .IsUnique();

        builder.Property(p => p.Theme)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("light");

        builder.Property(p => p.Language)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("en-IN");
    }
}
