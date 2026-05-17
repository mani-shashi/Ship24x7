using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ship24X7.Auth.Application.Commands;

namespace Ship24X7.Auth.API.Controllers;

/// <summary>
/// API controller for managing OAuth 2.0 authentication with external providers in the Ship24X7 auth service.
/// Handles HTTP requests for social login functionality using OAuth 2.0 authorization code flow.
/// Supports authentication with external providers like Google for seamless user onboarding.
/// Implements secure OAuth flow with state parameter for CSRF protection.
/// Workflow: Initiate OAuth -> Redirect to provider -> User authenticates -> Callback with code -> Exchange code for tokens -> Create/link account -> Issue JWT tokens.
/// No authentication required for these endpoints as they handle the authentication process itself.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class OAuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OAuthController> _logger;

    /// <summary>
    /// Initializes a new instance of the OAuthController class.
    /// Sets up dependencies for command processing and logging.
    /// </summary>
    /// <param name="mediator">MediatR instance for sending commands to their respective handlers using CQRS pattern</param>
    /// <param name="logger">Logger instance for recording OAuth operations, security events, errors, and diagnostic information</param>
    public OAuthController(IMediator mediator, ILogger<OAuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Initiates the OAuth 2.0 authentication flow with the specified external provider.
    /// Endpoint: GET /api/v1/oauth/{provider}?redirectUri={uri}&state={state}
    /// Logic flow:
    /// 1. Receives provider name from route parameter (e.g., "google")
    /// 2. Receives redirect URI and state parameter from query string
    /// 3. Creates InitiateOAuthCommand with provider, redirect URI, and state
    /// 4. Sends command to handler via MediatR for processing
    /// 5. Handler validates provider is supported (currently: Google)
    /// 6. If provider not supported, returns 400 Bad Request with "Provider not supported" message
    /// 7. Handler retrieves OAuth configuration from settings (client ID, client secret, scopes)
    /// 8. Handler constructs authorization URL with parameters:
    ///    - client_id: OAuth client ID from configuration
    ///    - redirect_uri: URI to redirect to after authentication
    ///    - response_type: "code" (authorization code flow)
    ///    - scope: "openid email profile" (requested user information)
    ///    - state: CSRF protection token (must match in callback)
    /// 9. Returns authorization URL for frontend to redirect user to
    /// Frontend redirects user to authorization URL where they authenticate with provider.
    /// After authentication, provider redirects back to redirect URI with authorization code.
    /// Used by frontend social login buttons to initiate OAuth flow.
    /// </summary>
    /// <param name="provider">OAuth provider name (e.g., "google"). Case-insensitive.</param>
    /// <param name="redirectUri">URI to redirect to after authentication. Must be registered with OAuth provider.</param>
    /// <param name="state">CSRF protection token. Frontend generates random string and validates in callback.</param>
    /// <returns>
    /// 200 OK with authorization URL to redirect user to for authentication.
    /// 400 Bad Request if provider not supported.
    /// 500 Internal Server Error for unexpected errors during OAuth initiation.
    /// </returns>
    [HttpGet("{provider}")]
    public async Task<IActionResult> Initiate(string provider, [FromQuery] string redirectUri, [FromQuery] string state)
    {
        try
        {
            var command = new InitiateOAuthCommand 
            { 
                Provider = provider,
                RedirectUri = redirectUri,
                State = state
            };

            var authorizationUrl = await _mediator.Send(command);
            return Ok(new { authorizationUrl });
        }
        catch (NotSupportedException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating OAuth");
            return StatusCode(500, new { error = "An error occurred during OAuth initiation" });
        }
    }

    /// <summary>
    /// Handles the OAuth 2.0 callback after user authenticates with external provider.
    /// Endpoint: POST /api/v1/oauth/callback
    /// Logic flow:
    /// 1. Receives authorization code, provider name, and state from request body
    /// 2. Creates CompleteOAuthCommand with code, provider, and state
    /// 3. Sends command to handler via MediatR for processing
    /// 4. Handler validates provider is supported
    /// 5. If provider not supported, returns 400 Bad Request
    /// 6. Handler validates state parameter matches original request (CSRF protection)
    /// 7. If state mismatch, returns 400 Bad Request with "Invalid state parameter" message
    /// 8. Handler exchanges authorization code for access token with provider's token endpoint
    /// 9. Handler uses access token to retrieve user information from provider's userinfo endpoint
    /// 10. Handler extracts email, name, and profile picture from user info
    /// 11. Handler checks if user with email already exists in database
    /// 12. If user exists:
    ///     a. Handler checks if ExternalLogin record exists for provider
    ///     b. If not exists, creates ExternalLogin record linking user to provider
    ///     c. Handler updates user's last login timestamp
    /// 13. If user does not exist:
    ///     a. Handler creates new User entity with email, name, EmailVerified=true (trusted provider)
    ///     b. Handler generates random password (user can reset later if needed)
    ///     c. Handler assigns default Customer_User role
    ///     d. Handler creates ExternalLogin record linking user to provider
    ///     e. Handler publishes UserRegistered domain event
    /// 14. Handler generates JWT access token with user ID, email, roles, and claims (expires in 15 minutes)
    /// 15. Handler generates refresh token (GUID) with 7-day expiration and saves to database
    /// 16. Sets refresh token in HttpOnly, Secure, SameSite=Strict cookie for security
    /// 17. Returns access token and expiration timestamp
    /// Used by frontend after OAuth provider redirects back with authorization code.
    /// Seamless user experience: No password required, automatic account creation if new user.
    /// </summary>
    /// <param name="command">Command containing authorization code, provider name, and state parameter from OAuth callback</param>
    /// <returns>
    /// 200 OK with access token and expiration if authentication succeeds.
    /// 400 Bad Request if provider not supported, state invalid, or authorization code invalid.
    /// 500 Internal Server Error for unexpected errors during OAuth completion.
    /// </returns>
    [HttpPost("callback")]
    public async Task<IActionResult> Callback([FromBody] CompleteOAuthCommand command)
    {
        try
        {
            var response = await _mediator.Send(command);

            // Set refresh token in HttpOnly cookie
            Response.Cookies.Append("refreshToken", response.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new 
            { 
                accessToken = response.AccessToken,
                expiresAt = response.ExpiresAt
            });
        }
        catch (NotSupportedException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing OAuth");
            return StatusCode(500, new { error = "An error occurred during OAuth completion" });
        }
    }
}
