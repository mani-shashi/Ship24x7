using Microsoft.EntityFrameworkCore;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Domain.Entities;
using Ship24X7.Auth.Infrastructure.Persistence;

namespace Ship24X7.Auth.Infrastructure.Repositories;

/// <summary>
/// Repository for managing verification token persistence and retrieval.
/// Handles tokens for email verification, phone verification, and password reset operations.
/// </summary>
public class VerificationTokenRepository : IVerificationTokenRepository
{
    private readonly AuthDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="VerificationTokenRepository"/> class.
    /// </summary>
    /// <param name="context">Database context for verification token operations.</param>
    public VerificationTokenRepository(AuthDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a verification token by its token string value.
    /// </summary>
    /// <param name="token">The verification token string to search for.</param>
    /// <returns>The verification token entity if found; otherwise, null.</returns>
    public async Task<VerificationToken?> GetByTokenAsync(string token)
    {
        return await _context.VerificationTokens
            .FirstOrDefaultAsync(vt => vt.Token == token);
    }

    /// <summary>
    /// Retrieves a verification token by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the verification token.</param>
    /// <returns>The verification token entity if found; otherwise, null.</returns>
    public async Task<VerificationToken?> GetByIdAsync(Guid id)
    {
        return await _context.VerificationTokens
            .FirstOrDefaultAsync(vt => vt.Id == id);
    }

    /// <summary>
    /// Adds a new verification token to the database.
    /// </summary>
    /// <param name="token">The verification token entity to add.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    public async Task AddAsync(VerificationToken token)
    {
        await _context.VerificationTokens.AddAsync(token);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing verification token in the database.
    /// </summary>
    /// <param name="token">The verification token entity with updated values.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    public async Task UpdateAsync(VerificationToken token)
    {
        _context.VerificationTokens.Update(token);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves all verification tokens for a specific user.
    /// Results are ordered by creation date in descending order (newest first).
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A list of verification tokens for the user.</returns>
    public async Task<List<VerificationToken>> GetByUserIdAsync(Guid userId)
    {
        return await _context.VerificationTokens
            .Where(vt => vt.UserId == userId)
            .OrderByDescending(vt => vt.CreatedAt)
            .ToListAsync();
    }
}
