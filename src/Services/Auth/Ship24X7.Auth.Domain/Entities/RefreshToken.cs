using Ship24X7.Shared.Domain;

namespace Ship24X7.Auth.Domain.Entities;

/// <summary>
/// Represents a refresh token for maintaining user sessions.
/// Implements token rotation and replay attack detection through token families.
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the refresh token.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the user ID this refresh token belongs to.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the refresh token value.
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the expiration time of the refresh token.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Gets or sets whether the refresh token has been revoked.
    /// </summary>
    public bool IsRevoked { get; set; }
    
    /// <summary>
    /// Gets or sets the time when the token was revoked.
    /// </summary>
    public DateTime? RevokedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the new token that replaced this token during rotation.
    /// Used for replay attack detection.
    /// </summary>
    public string? ReplacedByToken { get; set; }
    
    /// <summary>
    /// Gets or sets the token family identifier for tracking token lineage.
    /// All tokens in a rotation chain share the same family ID.
    /// </summary>
    public string TokenFamily { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user this refresh token belongs to.
    /// </summary>
    public User User { get; set; } = null!;
}
