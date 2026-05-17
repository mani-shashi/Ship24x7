using Ship24X7.Shared.Domain;

namespace Ship24X7.Auth.Domain.Entities;

/// <summary>
/// Represents an external OAuth login provider linked to a user account.
/// Stores OAuth provider information and tokens for third-party authentication.
/// </summary>
public class ExternalLogin : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the external login.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the user ID this external login belongs to.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the OAuth provider name (e.g., "Google", "Facebook").
    /// </summary>
    public string Provider { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the unique identifier from the OAuth provider.
    /// </summary>
    public string ProviderKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the display name of the OAuth provider.
    /// </summary>
    public string? ProviderDisplayName { get; set; }
    
    /// <summary>
    /// Gets or sets the access token from the OAuth provider.
    /// </summary>
    public string? AccessToken { get; set; }
    
    /// <summary>
    /// Gets or sets the refresh token from the OAuth provider.
    /// </summary>
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// Gets or sets the expiration time of the OAuth provider's access token.
    /// </summary>
    public DateTime? TokenExpiresAt { get; set; }
    
    /// <summary>
    /// Gets or sets the user this external login belongs to.
    /// </summary>
    public User User { get; set; } = null!;
}
