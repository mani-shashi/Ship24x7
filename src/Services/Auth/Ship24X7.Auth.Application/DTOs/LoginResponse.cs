namespace Ship24X7.Auth.Application.DTOs;

/// <summary>
/// Response data transfer object returned after successful login or token refresh.
/// Contains JWT access token, refresh token, expiration time, MFA requirement status, and user profile.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Gets or sets the JWT access token for API authentication.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the refresh token for obtaining new access tokens.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the expiration time of the access token.
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Gets or sets whether Multi-Factor Authentication is required to complete login.
    /// </summary>
    public bool RequiresMfa { get; set; }
    
    /// <summary>
    /// Gets or sets the user profile information.
    /// Null when RequiresMfa is true (user profile returned after MFA verification).
    /// </summary>
    public UserResponse? User { get; set; }
}
