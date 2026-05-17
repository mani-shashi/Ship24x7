using Ship24X7.Shared.Domain;

namespace Ship24X7.Auth.Domain.Events;

/// <summary>
/// Domain event raised when Multi-Factor Authentication is enabled for a user.
/// Can trigger security notifications or audit logging.
/// </summary>
public class MfaEnabled : BaseDomainEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the user who enabled MFA.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the email address of the user who enabled MFA.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
