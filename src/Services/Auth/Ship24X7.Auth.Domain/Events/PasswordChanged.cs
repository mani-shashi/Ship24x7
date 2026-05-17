using Ship24X7.Shared.Domain;

namespace Ship24X7.Auth.Domain.Events;

/// <summary>
/// Domain event raised when a user's password is changed.
/// Can trigger security notifications or force re-authentication.
/// </summary>
public class PasswordChanged : BaseDomainEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the user whose password was changed.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the email address of the user whose password was changed.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
