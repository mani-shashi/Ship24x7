using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Ship24X7.Auth.Application.Interfaces;

namespace Ship24X7.Auth.Infrastructure.Services;

/// <summary>
/// Google OAuth 2.0 service implementation for third-party authentication.
/// Handles authorization URL generation, code exchange, and user profile retrieval from Google.
/// </summary>
public class GoogleOAuthService : IOAuthService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleOAuthService"/> class.
    /// Loads Google OAuth configuration including client ID and secret.
    /// </summary>
    /// <param name="configuration">Application configuration containing Google OAuth settings.</param>
    /// <param name="httpClient">HTTP client for making requests to Google OAuth endpoints.</param>
    /// <exception cref="InvalidOperationException">Thrown when Google ClientId or ClientSecret is not configured.</exception>
    public GoogleOAuthService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _clientId = _configuration["OAuth:Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId not configured");
        _clientSecret = _configuration["OAuth:Google:ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret not configured");
    }

    /// <summary>
    /// Generates the Google OAuth authorization URL for user authentication.
    /// Includes openid, profile, and email scopes for accessing user information.
    /// </summary>
    /// <param name="provider">OAuth provider name (must be "google").</param>
    /// <param name="redirectUri">Callback URL after successful authentication.</param>
    /// <param name="state">CSRF protection state parameter.</param>
    /// <returns>Complete authorization URL for redirecting users to Google login.</returns>
    /// <exception cref="NotSupportedException">Thrown when provider is not "google".</exception>
    public string GetAuthorizationUrl(string provider, string redirectUri, string state)
    {
        if (provider.ToLower() != "google")
        {
            throw new NotSupportedException($"Provider {provider} is not supported");
        }

        var scope = "openid profile email";
        return $"https://accounts.google.com/o/oauth2/v2/auth?" +
               $"client_id={_clientId}&" +
               $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
               $"response_type=code&" +
               $"scope={Uri.EscapeDataString(scope)}&" +
               $"state={state}";
    }

    /// <summary>
    /// Exchanges authorization code for access token and retrieves user profile from Google.
    /// Performs token exchange and fetches user information including email, name, and provider ID.
    /// </summary>
    /// <param name="provider">OAuth provider name (must be "google").</param>
    /// <param name="code">Authorization code received from Google callback.</param>
    /// <param name="redirectUri">Callback URL used in authorization request.</param>
    /// <returns>OAuth user profile containing email, name, tokens, and expiration.</returns>
    /// <exception cref="NotSupportedException">Thrown when provider is not "google".</exception>
    public async Task<OAuthUserProfile> ExchangeCodeForProfileAsync(string provider, string code, string redirectUri)
    {
        if (provider.ToLower() != "google")
        {
            throw new NotSupportedException($"Provider {provider} is not supported");
        }

        // Exchange code for access token
        var tokenRequest = new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _clientId,
            ["client_secret"] = _clientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code"
        };

        var tokenResponse = await _httpClient.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(tokenRequest));

        tokenResponse.EnsureSuccessStatusCode();

        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenContent);

        var accessToken = tokenData.GetProperty("access_token").GetString();
        var refreshToken = tokenData.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
        var expiresIn = tokenData.GetProperty("expires_in").GetInt32();

        // Get user profile
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var profileResponse = await _httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
        profileResponse.EnsureSuccessStatusCode();

        var profileContent = await profileResponse.Content.ReadAsStringAsync();
        var profileData = JsonSerializer.Deserialize<JsonElement>(profileContent);

        return new OAuthUserProfile
        {
            ProviderId = profileData.GetProperty("id").GetString() ?? string.Empty,
            Email = profileData.GetProperty("email").GetString() ?? string.Empty,
            FullName = profileData.GetProperty("name").GetString() ?? string.Empty,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn)
        };
    }
}
