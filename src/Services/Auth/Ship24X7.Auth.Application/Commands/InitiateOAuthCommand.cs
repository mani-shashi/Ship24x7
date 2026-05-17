using MediatR;

namespace Ship24X7.Auth.Application.Commands;

/// <summary>
/// Command to initiate OAuth authentication flow with an external provider.
/// Generates the authorization URL for redirecting users to the OAuth provider.
/// </summary>
public class InitiateOAuthCommand : IRequest<string>
{
    /// <summary>
    /// Gets or sets the OAuth provider name (e.g., "Google", "Facebook").
    /// </summary>
    public string Provider { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the URI to redirect to after OAuth authentication completes.
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the state parameter for CSRF protection during OAuth flow.
    /// </summary>
    public string State { get; set; } = string.Empty;
}
