using Microsoft.EntityFrameworkCore;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Domain.Entities;
using Ship24X7.Auth.Infrastructure.Persistence;

namespace Ship24X7.Auth.Infrastructure.Repositories;

/// <summary>
/// Repository for managing refresh token persistence and lifecycle operations.
/// Handles token storage, retrieval, revocation, and family-based token management.
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenRepository"/> class.
    /// </summary>
    /// <param name="context">Database context for refresh token operations.</param>
    public RefreshTokenRepository(AuthDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a refresh token by its token string value.
    /// Includes associated user information in the result.
    /// </summary>
    /// <param name="token">The refresh token string to search for.</param>
    /// <returns>The refresh token entity if found; otherwise, null.</returns>
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    /// <summary>
    /// Adds a new refresh token to the database.
    /// </summary>
    /// <param name="refreshToken">The refresh token entity to add.</param>
    /// <returns>The added refresh token entity with generated ID.</returns>
    public async Task<RefreshToken> AddAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }

    /// <summary>
    /// Updates an existing refresh token in the database.
    /// </summary>
    /// <param name="refreshToken">The refresh token entity with updated values.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Revokes all active refresh tokens for a specific user.
    /// Used during logout or security events to invalidate all user sessions.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A task representing the asynchronous revocation operation.</returns>
    public async Task RevokeAllForUserAsync(Guid userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Revokes all tokens in a token family to prevent token reuse attacks.
    /// Used when token rotation detects a compromised token family.
    /// </summary>
    /// <param name="tokenFamily">The token family identifier.</param>
    /// <returns>A task representing the asynchronous revocation operation.</returns>
    public async Task RevokeTokenFamilyAsync(string tokenFamily)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.TokenFamily == tokenFamily && !rt.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}
