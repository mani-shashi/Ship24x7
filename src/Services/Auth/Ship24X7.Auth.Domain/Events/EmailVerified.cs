using Ship24X7.Shared.Domain;

namespace Ship24X7.Auth.Domain.Events;

/// <summary>
/// Domain event raised when a user's email address is successfully verified.
/// Can trigger notifications or other business logic.
/// </summary>
public class EmailVerified : BaseDomainEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the user whose email was verified.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the verified email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
