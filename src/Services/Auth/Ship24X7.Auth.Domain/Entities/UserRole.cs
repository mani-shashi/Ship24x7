using Ship24X7.Shared.Domain;

namespace Ship24X7.Auth.Domain.Entities;

/// <summary>
/// Represents the many-to-many relationship between users and roles.
/// Associates users with their assigned roles for access control.
/// </summary>
public class UserRole : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the user-role association.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the user ID in the association.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the role ID in the association.
    /// </summary>
    public Guid RoleId { get; set; }
    
    /// <summary>
    /// Gets or sets the user in the association.
    /// </summary>
    public User User { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the role in the association.
    /// </summary>
    public Role Role { get; set; } = null!;
}
