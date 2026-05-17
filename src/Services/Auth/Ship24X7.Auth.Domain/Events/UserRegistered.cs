using Ship24X7.Shared.Domain;

namespace Ship24X7.Auth.Domain.Events;

/// <summary>
/// Domain event raised when a new user registers in the system.
/// Can trigger welcome emails, analytics, or onboarding workflows.
/// </summary>
public class UserRegistered : BaseDomainEvent
{
    /// <summary>
    /// Gets or sets the unique identifier of the newly registered user.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the email address of the newly registered user.
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the full name of the newly registered user.
    /// </summary>
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the email verification token sent to the user.
    /// </summary>
    public string VerificationToken { get; set; } = string.Empty;
}
