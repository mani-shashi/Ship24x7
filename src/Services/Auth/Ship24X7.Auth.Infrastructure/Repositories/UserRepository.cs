using Microsoft.EntityFrameworkCore;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Domain.Entities;
using Ship24X7.Auth.Infrastructure.Persistence;

namespace Ship24X7.Auth.Infrastructure.Repositories;

/// <summary>
/// Repository for managing user persistence and retrieval operations.
/// Handles user CRUD operations, queries with related entities, and email existence checks.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="context">Database context for user operations.</param>
    public UserRepository(AuthDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// Includes MFA settings, roles, and claims in the result.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>The user entity if found; otherwise, null.</returns>
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.MfaSettings)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserClaims)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <summary>
    /// Retrieves a user by their unique identifier with roles and MFA settings eagerly loaded.
    /// Alias for GetByIdAsync - both methods return the same data.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>The user entity with roles and MFA settings if found; otherwise, null.</returns>
    public async Task<User?> GetByIdWithRolesAsync(Guid id)
    {
        return await GetByIdAsync(id);
    }

    /// <summary>
    /// Retrieves a user by their email address.
    /// Email comparison is case-insensitive.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <returns>The user entity if found; otherwise, null.</returns>
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());
    }

    /// <summary>
    /// Retrieves a user by email with all related authentication data.
    /// Includes MFA settings, roles, claims, and external logins for complete authentication context.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <returns>The user entity with all related data if found; otherwise, null.</returns>
    public async Task<User?> GetByEmailWithRolesAsync(string email)
    {
        return await _context.Users
            .Include(u => u.MfaSettings)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.UserClaims)
            .Include(u => u.ExternalLogins)
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());
    }

    /// <summary>
    /// Retrieves a paginated list of users with their roles.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of users per page.</param>
    /// <returns>A collection of user entities for the specified page.</returns>
    public async Task<IEnumerable<User>> GetAllAsync(int page, int pageSize)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Adds a new user to the database.
    /// </summary>
    /// <param name="user">The user entity to add.</param>
    /// <returns>The added user entity with generated ID.</returns>
    public async Task<User> AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Updates an existing user in the database.
    /// </summary>
    /// <param name="user">The user entity with updated values.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Soft deletes a user by marking them as deleted.
    /// User data is retained but marked inactive with deletion timestamp.
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    public async Task DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Checks if an email address is already registered in the system.
    /// Email comparison is case-insensitive.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <returns>True if the email exists; otherwise, false.</returns>
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email.ToLowerInvariant());
    }
}
