using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Application.Interfaces;

/// <summary>
/// Repository interface for user data operations.
/// Handles user CRUD operations, queries, and email existence checks.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="id">The user ID to search for.</param>
    /// <returns>The user entity if found; otherwise, null.</returns>
    Task<User?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Retrieves a user by their unique identifier with roles and MFA settings eagerly loaded.
    /// </summary>
    /// <param name="id">The user ID to search for.</param>
    /// <returns>The user entity with roles and MFA settings if found; otherwise, null.</returns>
    Task<User?> GetByIdWithRolesAsync(Guid id);
    
    /// <summary>
    /// Retrieves a user by their email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The user entity if found; otherwise, null.</returns>
    Task<User?> GetByEmailAsync(string email);
    
    /// <summary>
    /// Retrieves a user by email with their roles eagerly loaded.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The user entity with roles if found; otherwise, null.</returns>
    Task<User?> GetByEmailWithRolesAsync(string email);
    
    /// <summary>
    /// Retrieves a paginated list of all users.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of users per page.</param>
    /// <returns>A collection of user entities for the specified page.</returns>
    Task<IEnumerable<User>> GetAllAsync(int page, int pageSize);
    
    /// <summary>
    /// Adds a new user to the repository.
    /// </summary>
    /// <param name="user">The user entity to add.</param>
    /// <returns>The added user entity.</returns>
    Task<User> AddAsync(User user);
    
    /// <summary>
    /// Updates an existing user in the repository.
    /// </summary>
    /// <param name="user">The user entity to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(User user);
    
    /// <summary>
    /// Deletes a user from the repository.
    /// </summary>
    /// <param name="id">The user ID to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Checks if an email address is already registered.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <returns>True if the email exists; otherwise, false.</returns>
    Task<bool> EmailExistsAsync(string email);
}
