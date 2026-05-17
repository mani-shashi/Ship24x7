using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Application.Interfaces;

/// <summary>
/// Repository interface for refresh token data operations.
/// Handles token storage, retrieval, revocation, and family management.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Retrieves a refresh token by its token value.
    /// </summary>
    /// <param name="token">The refresh token string to search for.</param>
    /// <returns>The refresh token entity if found; otherwise, null.</returns>
    Task<RefreshToken?> GetByTokenAsync(string token);
    
    /// <summary>
    /// Adds a new refresh token to the repository.
    /// </summary>
    /// <param name="refreshToken">The refresh token entity to add.</param>
    /// <returns>The added refresh token entity.</returns>
    Task<RefreshToken> AddAsync(RefreshToken refreshToken);
    
    /// <summary>
    /// Updates an existing refresh token in the repository.
    /// </summary>
    /// <param name="refreshToken">The refresh token entity to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(RefreshToken refreshToken);
    
    /// <summary>
    /// Revokes all refresh tokens for a specific user.
    /// </summary>
    /// <param name="userId">The user ID whose tokens should be revoked.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RevokeAllForUserAsync(Guid userId);
    
    /// <summary>
    /// Revokes all tokens in a token family to prevent replay attacks.
    /// </summary>
    /// <param name="tokenFamily">The token family identifier.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RevokeTokenFamilyAsync(string tokenFamily);
}
