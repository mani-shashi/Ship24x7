using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Application.Interfaces;

/// <summary>
/// Repository interface for verification token data operations.
/// Handles token storage, retrieval, and updates for email/phone verification and password reset.
/// </summary>
public interface IVerificationTokenRepository
{
    /// <summary>
    /// Retrieves a verification token by its token value.
    /// </summary>
    /// <param name="token">The token string to search for.</param>
    /// <returns>The verification token entity if found; otherwise, null.</returns>
    Task<VerificationToken?> GetByTokenAsync(string token);
    
    /// <summary>
    /// Retrieves a verification token by its unique identifier.
    /// </summary>
    /// <param name="id">The token ID to search for.</param>
    /// <returns>The verification token entity if found; otherwise, null.</returns>
    Task<VerificationToken?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Adds a new verification token to the repository.
    /// </summary>
    /// <param name="token">The verification token entity to add.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(VerificationToken token);
    
    /// <summary>
    /// Updates an existing verification token in the repository.
    /// </summary>
    /// <param name="token">The verification token entity to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(VerificationToken token);
    
    /// <summary>
    /// Retrieves all verification tokens for a specific user.
    /// </summary>
    /// <param name="userId">The user ID to search for.</param>
    /// <returns>A list of verification token entities for the user.</returns>
    Task<List<VerificationToken>> GetByUserIdAsync(Guid userId);
}
