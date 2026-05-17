using MediatR;

namespace Ship24X7.Auth.Application.Commands;

/// <summary>
/// Command for registering a new user account in the Ship24X7 authentication system.
/// Encapsulates all required user registration data including credentials and contact information.
/// Implements IRequest pattern from MediatR for CQRS architecture.
/// Returns user ID (GUID) upon successful registration.
/// Triggers email verification workflow by generating token and sending verification email.
/// User account created with IsActive=false and EmailVerified=false until email is verified.
/// Password is hashed using BCrypt before storage for security.
/// </summary>
public class RegisterUserCommand : IRequest<Guid>
{
    /// <summary>
    /// Gets or sets the user's email address for account identification and communication.
    /// Must be unique in the system - registration fails if email already exists.
    /// Used as username for login and for sending verification/notification emails.
    /// Validation rules: Valid email format, not null or empty, max 255 characters.
    /// Example: "customer@example.com"
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user's password for account authentication.
    /// Will be hashed using BCrypt with salt rounds before storage in database.
    /// Never stored in plain text for security.
    /// Validation rules: Minimum 8 characters, at least one uppercase letter, one lowercase letter, one digit, one special character.
    /// Example: "SecureP@ss123"
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user's full name for display and personalization purposes.
    /// Used in email greetings, dashboard displays, and customer service interactions.
    /// Validation rules: Not null or empty, max 100 characters.
    /// Example: "John Smith"
    /// </summary>
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user's phone number for contact and SMS notifications.
    /// Used for account recovery, order notifications, and customer support.
    /// Validation rules: Valid phone number format, 10-15 digits.
    /// Example: "+919876543210" or "9876543210"
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
}
