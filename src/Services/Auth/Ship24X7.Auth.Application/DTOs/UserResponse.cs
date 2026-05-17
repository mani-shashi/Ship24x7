namespace Ship24X7.Auth.Application.DTOs;

/// <summary>
/// Response data transfer object for user profile information.
/// Contains complete user details including roles and verification status.
/// Used by /me endpoint and other user profile operations.
/// </summary>
public class UserResponse
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user's full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user's phone number.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets whether the user's email has been verified.
    /// </summary>
    public bool EmailVerified { get; set; }
    
    /// <summary>
    /// Gets or sets whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Gets or sets the list of roles assigned to the user.
    /// </summary>
    public List<string> Roles { get; set; } = new();
    
    /// <summary>
    /// Gets or sets whether Multi-Factor Authentication is enabled for the user.
    /// </summary>
    public bool MfaEnabled { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time when the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the date and time of the user's last login.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}
