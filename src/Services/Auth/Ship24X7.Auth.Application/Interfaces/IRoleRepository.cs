using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Application.Interfaces;

/// <summary>
/// Repository interface for role data operations.
/// Handles role retrieval and creation for role-based access control.
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Retrieves a role by its unique identifier.
    /// </summary>
    /// <param name="id">The role ID to search for.</param>
    /// <returns>The role entity if found; otherwise, null.</returns>
    Task<Role?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Retrieves a role by its name.
    /// </summary>
    /// <param name="name">The role name to search for.</param>
    /// <returns>The role entity if found; otherwise, null.</returns>
    Task<Role?> GetByNameAsync(string name);
    
    /// <summary>
    /// Retrieves all roles in the system.
    /// </summary>
    /// <returns>A collection of all role entities.</returns>
    Task<IEnumerable<Role>> GetAllAsync();
    
    /// <summary>
    /// Adds a new role to the repository.
    /// </summary>
    /// <param name="role">The role entity to add.</param>
    /// <returns>The added role entity.</returns>
    Task<Role> AddAsync(Role role);
}
