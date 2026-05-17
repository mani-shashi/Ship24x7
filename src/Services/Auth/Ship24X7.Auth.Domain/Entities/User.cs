using Ship24X7.Shared.Domain;

namespace Ship24X7.Auth.Domain.Entities;

/// <summary>
/// Represents a user account in the Ship24X7 authentication system.
/// Contains user credentials, profile information, security settings, and relationships to authentication-related entities.
/// Supports email/phone verification, account locking, MFA, and external OAuth logins.
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the user's email address used for authentication and communication.
    /// Must be unique across the system.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the hashed password for the user account.
    /// Stored using a secure hashing algorithm (e.g., BCrypt).
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user's full name for display and identification purposes.
    /// </summary>
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user's phone number for contact and optional SMS-based authentication.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets a value indicating whether the user's email address has been verified.
    /// Users must verify their email before accessing certain features.
    /// </summary>
    public bool EmailVerified { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the user's phone number has been verified.
    /// </summary>
    public bool PhoneVerified { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the user account is active.
    /// Inactive accounts cannot log in to the system.
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Gets or sets a value indicating whether the user account is locked due to security reasons.
    /// Locked accounts cannot log in until unlocked by an administrator.
    /// </summary>
    public bool IsLocked { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when the account lockout expires.
    /// Null if the account is not locked or locked indefinitely.
    /// </summary>
    public DateTime? LockoutEnd { get; set; }
    
    /// <summary>
    /// Gets or sets the number of consecutive failed login attempts.
    /// Used to implement account lockout after too many failed attempts.
    /// </summary>
    public int FailedLoginAttempts { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time of the user's last successful login.
    /// Null if the user has never logged in.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>
    /// Gets or sets the collection of refresh tokens associated with this user.
    /// Used for JWT token refresh functionality across multiple devices/sessions.
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    
    /// <summary>
    /// Gets or sets the collection of external OAuth logins linked to this user account.
    /// Enables social login functionality (e.g., Google, Facebook).
    /// </summary>
    public ICollection<ExternalLogin> ExternalLogins { get; set; } = new List<ExternalLogin>();
    
    /// <summary>
    /// Gets or sets the multi-factor authentication settings for this user.
    /// Null if MFA is not enabled for the account.
    /// </summary>
    public MfaSettings? MfaSettings { get; set; }
    
    /// <summary>
    /// Gets or sets the collection of verification tokens for email/phone verification.
    /// </summary>
    public ICollection<VerificationToken> VerificationTokens { get; set; } = new List<VerificationToken>();
    
    /// <summary>
    /// Gets or sets the collection of roles assigned to this user.
    /// Defines the user's permissions and access levels in the system.
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    
    /// <summary>
    /// Gets or sets the collection of custom claims associated with this user.
    /// Used for fine-grained authorization and storing additional user metadata.
    /// </summary>
    public ICollection<UserClaim> UserClaims { get; set; } = new List<UserClaim>();

    /// <summary>
    /// Gets or sets the collection of saved addresses in the user's address book.
    /// </summary>
    public ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();

    /// <summary>
    /// Gets or sets the user's notification and display preferences.
    /// Null until the user first saves preferences.
    /// </summary>
    public UserPreferences? Preferences { get; set; }

    /// <summary>
    /// Gets or sets the URL of the user's profile photo.
    /// Null if no photo has been uploaded.
    /// </summary>
    public string? ProfilePhotoUrl { get; set; }
}
