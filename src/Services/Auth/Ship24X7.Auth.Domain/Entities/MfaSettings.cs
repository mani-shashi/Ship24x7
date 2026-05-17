using Ship24X7.Shared.Domain;

namespace Ship24X7.Auth.Domain.Entities;

/// <summary>
/// Represents Multi-Factor Authentication settings for a user.
/// Stores TOTP secret, backup codes, and lockout information for MFA.
/// </summary>
public class MfaSettings : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the MFA settings.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the user ID these MFA settings belong to.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets whether MFA is enabled for the user.
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// Gets or sets the TOTP secret key for generating time-based codes.
    /// </summary>
    public string TotpSecret { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the backup codes for account recovery when MFA device is unavailable.
    /// </summary>
    public string[] BackupCodes { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// Gets or sets the number of consecutive failed MFA attempts.
    /// </summary>
    public int FailedAttempts { get; set; }
    
    /// <summary>
    /// Gets or sets the time until which MFA is locked due to failed attempts.
    /// </summary>
    public DateTime? LockedUntil { get; set; }
    
    /// <summary>
    /// Gets or sets the user these MFA settings belong to.
    /// </summary>
    public User User { get; set; } = null!;
}
