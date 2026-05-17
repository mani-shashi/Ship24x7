using Ship24X7.Shared.Domain;

namespace Ship24X7.Auth.Domain.Entities;

/// <summary>
/// Represents a custom claim associated with a user.
/// Stores additional user attributes for fine-grained authorization.
/// </summary>
public class UserClaim : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the user claim.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the user ID this claim belongs to.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the claim type (e.g., "Department", "Region").
    /// </summary>
    public string ClaimType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the claim value (e.g., "Sales", "North").
    /// </summary>
    public string ClaimValue { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user this claim belongs to.
    /// </summary>
    public User User { get; set; } = null!;
}
