namespace Ship24X7.Auth.Application.Interfaces;

/// <summary>
/// Service interface for OAuth authentication with external providers.
/// Handles authorization URL generation and token exchange for user profiles.
/// </summary>
public interface IOAuthService
{
    /// <summary>
    /// Generates the OAuth authorization URL for redirecting users to the provider.
    /// </summary>
    /// <param name="provider">The OAuth provider name (e.g., "Google", "Facebook").</param>
    /// <param name="redirectUri">The URI to redirect to after authentication.</param>
    /// <param name="state">The state parameter for CSRF protection.</param>
    /// <returns>The authorization URL to redirect the user to.</returns>
    string GetAuthorizationUrl(string provider, string redirectUri, string state);
    
    /// <summary>
    /// Exchanges the authorization code for the user's profile information.
    /// </summary>
    /// <param name="provider">The OAuth provider name.</param>
    /// <param name="code">The authorization code received from the provider.</param>
    /// <param name="redirectUri">The redirect URI used in the authorization request.</param>
    /// <returns>The user's profile information from the OAuth provider.</returns>
    Task<OAuthUserProfile> ExchangeCodeForProfileAsync(string provider, string code, string redirectUri);
}

/// <summary>
/// Represents user profile information retrieved from an OAuth provider.
/// </summary>
public class OAuthUserProfile
{
    /// <summary>
    /// Gets or sets the unique identifier from the OAuth provider.
    /// </summary>
    public string ProviderId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user's email address from the OAuth provider.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user's full name from the OAuth provider.
    /// </summary>
    public string FullName { get; set; } = string.Empty;
    
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
}
