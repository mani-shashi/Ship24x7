using Microsoft.EntityFrameworkCore;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Domain.Entities;
using Ship24X7.Auth.Infrastructure.Persistence;

namespace Ship24X7.Auth.Infrastructure.Repositories;

/// <summary>
/// Repository for managing role persistence and retrieval operations.
/// Handles role CRUD operations for role-based access control.
/// </summary>
public class RoleRepository : IRoleRepository
{
    private readonly AuthDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleRepository"/> class.
    /// </summary>
    /// <param name="context">Database context for role operations.</param>
    public RoleRepository(AuthDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a role by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the role.</param>
    /// <returns>The role entity if found; otherwise, null.</returns>
    public async Task<Role?> GetByIdAsync(Guid id)
    {
        return await _context.Roles.FindAsync(id);
    }

    /// <summary>
    /// Retrieves a role by its name.
    /// </summary>
    /// <param name="name">The name of the role (e.g., "Admin", "Customer").</param>
    /// <returns>The role entity if found; otherwise, null.</returns>
    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == name);
    }

    /// <summary>
    /// Retrieves all roles from the database.
    /// </summary>
    /// <returns>A collection of all role entities.</returns>
    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    /// <summary>
    /// Adds a new role to the database.
    /// </summary>
    /// <param name="role">The role entity to add.</param>
    /// <returns>The added role entity with generated ID.</returns>
    public async Task<Role> AddAsync(Role role)
    {
        await _context.Roles.AddAsync(role);
        await _context.SaveChangesAsync();
        return role;
    }
}
