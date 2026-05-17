using Microsoft.EntityFrameworkCore;
using Ship24X7.Auth.Domain.Entities;
using Ship24X7.Auth.Infrastructure.Persistence.Configurations;

namespace Ship24X7.Auth.Infrastructure.Persistence;

/// <summary>
/// Database context for authentication and authorization entities.
/// Manages users, roles, permissions, tokens, and MFA settings with Entity Framework Core.
/// </summary>
public class AuthDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthDbContext"/> class.
    /// </summary>
    /// <param name="options">Database context options including connection string and provider.</param>
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets the users table containing all registered user accounts.
    /// </summary>
    public DbSet<User> Users => Set<User>();
    
    /// <summary>
    /// Gets the roles table containing system roles for access control.
    /// </summary>
    public DbSet<Role> Roles => Set<Role>();
    
    /// <summary>
    /// Gets the user-role mapping table for many-to-many relationship.
    /// </summary>
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    
    /// <summary>
    /// Gets the user claims table for custom permissions and attributes.
    /// </summary>
    public DbSet<UserClaim> UserClaims => Set<UserClaim>();
    
    /// <summary>
    /// Gets the refresh tokens table for token rotation and session management.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
    /// <summary>
    /// Gets the external logins table for OAuth provider associations.
    /// </summary>
    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();
    
    /// <summary>
    /// Gets the MFA settings table for two-factor authentication configuration.
    /// </summary>
    public DbSet<MfaSettings> MfaSettings => Set<MfaSettings>();
    
    /// <summary>
    /// Gets the verification tokens table for email and password reset operations.
    /// </summary>
    public DbSet<VerificationToken> VerificationTokens => Set<VerificationToken>();

    /// <summary>
    /// Gets the user addresses table for the address book feature.
    /// </summary>
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();

    /// <summary>
    /// Gets the user preferences table for notification and display settings.
    /// </summary>
    public DbSet<UserPreferences> UserPreferences => Set<UserPreferences>();

    /// <summary>
    /// Configures entity relationships, constraints, and indexes using Fluent API.
    /// Applies all entity type configurations from the Configurations folder.
    /// </summary>
    /// <param name="modelBuilder">Model builder for configuring entity mappings.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserClaimConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new ExternalLoginConfiguration());
        modelBuilder.ApplyConfiguration(new MfaSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new VerificationTokenConfiguration());
        modelBuilder.ApplyConfiguration(new UserAddressConfiguration());
        modelBuilder.ApplyConfiguration(new UserPreferencesConfiguration());
    }
}
