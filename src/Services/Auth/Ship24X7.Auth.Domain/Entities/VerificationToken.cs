using Ship24X7.Shared.Domain;

namespace Ship24X7.Auth.Domain.Entities;

/// <summary>
/// Represents a verification token for email, phone, or password reset verification.
/// Tokens are single-use and have expiration times for security.
/// </summary>
public class VerificationToken : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the verification token.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the user ID this verification token belongs to.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the token value sent to the user.
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the type of verification this token is for.
    /// </summary>
    public VerificationTokenType Type { get; set; }
    
    /// <summary>
    /// Gets or sets the expiration time of the token.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Gets or sets whether the token has been used.
    /// </summary>
    public bool IsUsed { get; set; }
    
    /// <summary>
    /// Gets or sets the time when the token was used.
    /// </summary>
    public DateTime? UsedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the user this verification token belongs to.
    /// </summary>
    public User User { get; set; } = null!;
}

/// <summary>
/// Defines the types of verification tokens available in the system.
/// </summary>
public enum VerificationTokenType
{
    /// <summary>
    /// Token for verifying email addresses.
    /// </summary>
    EmailVerification,
    
    /// <summary>
    /// Token for verifying phone numbers.
    /// </summary>
    PhoneVerification,
    
    /// <summary>
    /// Token for resetting forgotten passwords.
    /// </summary>
    PasswordReset
}
