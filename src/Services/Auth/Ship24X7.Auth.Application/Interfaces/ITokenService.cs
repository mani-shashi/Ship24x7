namespace Ship24X7.Auth.Application.Interfaces;

/// <summary>
/// Service interface for JWT token generation and validation.
/// Handles access token and refresh token operations.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token with user claims, roles, and custom claims.
    /// </summary>
    /// <param name="userId">The user's unique identifier.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="roles">The user's roles for authorization.</param>
    /// <param name="claims">Additional custom claims to include in the token.</param>
    /// <returns>A JWT access token string.</returns>
    string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles, IEnumerable<string> claims);
    
    /// <summary>
    /// Generates a cryptographically secure refresh token.
    /// </summary>
    /// <returns>A refresh token string.</returns>
    string GenerateRefreshToken();
    
    /// <summary>
    /// Validates a JWT token's signature and expiration.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <returns>True if the token is valid; otherwise, false.</returns>
    bool ValidateToken(string token);
    
    /// <summary>
    /// Extracts the user ID from a JWT token.
    /// </summary>
    /// <param name="token">The JWT token to extract from.</param>
    /// <returns>The user ID if found; otherwise, null.</returns>
    Guid? GetUserIdFromToken(string token);
}
