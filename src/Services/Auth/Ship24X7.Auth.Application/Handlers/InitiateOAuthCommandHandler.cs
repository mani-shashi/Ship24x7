using MediatR;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.Interfaces;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Handles OAuth flow initiation by generating the authorization URL for the provider.
/// Constructs the URL with client ID, redirect URI, scopes, and state for CSRF protection.
/// </summary>
public class InitiateOAuthCommandHandler : IRequestHandler<InitiateOAuthCommand, string>
{
    private readonly IOAuthService _oauthService;

    /// <summary>
    /// Initializes a new instance of the InitiateOAuthCommandHandler class.
    /// </summary>
    /// <param name="oauthService">Service for OAuth provider operations.</param>
    public InitiateOAuthCommandHandler(IOAuthService oauthService)
    {
        _oauthService = oauthService;
    }

    /// <summary>
    /// Generates the OAuth authorization URL for redirecting users to the provider's login page.
    /// Process flow:
    /// 1. Receives provider name (e.g., "Google"), redirect URI, and state parameter
    /// 2. Delegates to OAuth service to construct provider-specific authorization URL
    /// 3. URL includes client ID, redirect URI, response type (code), scopes (openid, profile, email), and state
    /// 4. Returns complete URL for client-side redirect
    /// State parameter provides CSRF protection by validating callback authenticity.
    /// </summary>
    /// <param name="request">The initiate OAuth command with provider name, redirect URI, and CSRF state.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The complete authorization URL to redirect the user to for OAuth authentication.</returns>
    public Task<string> Handle(InitiateOAuthCommand request, CancellationToken cancellationToken)
    {
        var authorizationUrl = _oauthService.GetAuthorizationUrl(
            request.Provider,
            request.RedirectUri,
            request.State);

        return Task.FromResult(authorizationUrl);
    }
}
