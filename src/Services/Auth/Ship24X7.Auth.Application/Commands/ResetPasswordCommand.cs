using MediatR;

namespace Ship24X7.Auth.Application.Commands;

/// <summary>
/// Command for resetting a user's password using a valid reset token.
/// Validates the reset token and updates the user's password.
/// Implements IRequest pattern from MediatR for CQRS architecture.
/// Returns boolean indicating successful password reset.
/// Token must be valid (not expired, not used) and password must meet strength requirements.
/// </summary>
public class ResetPasswordCommand : IRequest<bool>
{
    /// <summary>
    /// Gets or sets the password reset token sent to the user's email.
    /// Generated during forgot password request with 1-hour expiration.
    /// Included in reset email link: /reset-password?token={token}
    /// Handler validates token exists in database and not expired.
    /// Token deleted after successful password reset for security.
    /// Required field. Example: "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
    /// </summary>
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the new password for the user account.
    /// Will be hashed using BCrypt before storage in database.
    /// Must meet password strength requirements:
    /// - Minimum 8 characters
    /// - At least one uppercase letter
    /// - At least one lowercase letter
    /// - At least one digit
    /// - At least one special character
    /// Required field. Example: "NewSecure@Pass123"
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;
}
