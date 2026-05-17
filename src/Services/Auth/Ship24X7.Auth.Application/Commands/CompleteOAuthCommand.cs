using MediatR;
using Ship24X7.Auth.Application.DTOs;

namespace Ship24X7.Auth.Application.Commands;

/// <summary>
/// Command to complete OAuth authentication flow after provider callback.
/// Exchanges authorization code for tokens and creates or authenticates user.
/// </summary>
public class CompleteOAuthCommand : IRequest<LoginResponse>
{
    /// <summary>
    /// Gets or sets the OAuth provider name (e.g., "Google", "Facebook").
    /// </summary>
    public string Provider { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the authorization code received from the OAuth provider.
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the redirect URI used in the OAuth flow for validation.
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the state parameter for CSRF validation.
    /// </summary>
    public string State { get; set; } = string.Empty;
}
