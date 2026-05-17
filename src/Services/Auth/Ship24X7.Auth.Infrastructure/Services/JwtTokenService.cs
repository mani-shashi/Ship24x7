using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Ship24X7.Auth.Application.Interfaces;

namespace Ship24X7.Auth.Infrastructure.Services;

/// <summary>
/// JWT token service implementation for generating and validating access and refresh tokens.
/// Handles token creation with claims, roles, and permissions using HMAC-SHA256 signing.
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenService"/> class.
    /// Loads JWT configuration including secret key, issuer, and audience from app settings.
    /// </summary>
    /// <param name="configuration">Application configuration containing JWT settings.</param>
    /// <exception cref="InvalidOperationException">Thrown when JWT SecretKey is not configured.</exception>
    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        _issuer = _configuration["Jwt:Issuer"] ?? "Ship24X7";
        _audience = _configuration["Jwt:Audience"] ?? "Ship24X7";
    }

    /// <summary>
    /// Generates a JWT access token with user identity, roles, and custom claims.
    /// Token expires in 15 minutes and is signed with HMAC-SHA256.
    /// </summary>
    /// <param name="userId">Unique identifier of the user.</param>
    /// <param name="email">User's email address.</param>
    /// <param name="roles">Collection of role names assigned to the user.</param>
    /// <param name="claims">Collection of custom permission claims.</param>
    /// <returns>Base64-encoded JWT access token string.</returns>
    public string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles, IEnumerable<string> claims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        var claimsList = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add roles
        foreach (var role in roles)
        {
            claimsList.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add custom claims
        foreach (var claim in claims)
        {
            claimsList.Add(new Claim("permission", claim));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claimsList),
            Expires = DateTime.UtcNow.AddMinutes(15),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Generates a cryptographically secure random refresh token.
    /// Uses 64 bytes of random data encoded as Base64 string.
    /// </summary>
    /// <returns>Base64-encoded refresh token string.</returns>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Validates a JWT token's signature, issuer, audience, and expiration.
    /// Uses zero clock skew for strict expiration validation.
    /// </summary>
    /// <param name="token">JWT token string to validate.</param>
    /// <returns>True if token is valid; otherwise, false.</returns>
    public bool ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extracts the user ID from a JWT token without validating the token.
    /// Searches for user ID in nameid, sub, or NameIdentifier claims.
    /// </summary>
    /// <param name="token">JWT token string to parse.</param>
    /// <returns>User ID if found and valid; otherwise, null.</returns>
    public Guid? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            // JWT tokens serialize ClaimTypes.NameIdentifier as "nameid" or "sub"
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid" || c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
