using MediatR;

namespace Ship24X7.Auth.Application.Commands;

/// <summary>
/// Command for initiating a password reset request.
/// Generates a password reset token and sends it to the user's email.
/// Implements IRequest pattern from MediatR for CQRS architecture.
/// Returns boolean indicating whether the email was sent successfully.
/// Token expires in 1 hour for security.
/// </summary>
public class ForgotPasswordCommand : IRequest<bool>
{
    /// <summary>
    /// Gets or sets the email address of the user requesting password reset.
    /// Used to find the user account and send the reset token.
    /// Required field. Example: "user@example.com"
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
