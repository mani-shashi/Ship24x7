using Ship24X7.Shared.Domain;

namespace Ship24X7.Auth.Domain.Entities;

/// <summary>
/// Represents a user role in the system for role-based access control.
/// Defines permissions and access levels for different user types.
/// </summary>
public class Role : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the role.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the role name (e.g., "Customer", "Admin", "HubManager").
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the description of the role's purpose and permissions.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the collection of user-role associations.
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
