using MediatR;

namespace Ship24X7.Auth.Application.Commands;

/// <summary>
/// Command for verifying a user's email address using a verification token.
/// Encapsulates verification token sent to user's email during registration.
/// Implements IRequest pattern from MediatR for CQRS architecture.
/// Returns boolean indicating successful verification (true) or failure (false).
/// Validates token exists, not expired (24-hour lifetime), and belongs to unverified user.
/// Marks user's email as verified (EmailVerified=true) and deletes verification token.
/// User can login after email verification.
/// Publishes EmailVerified domain event for downstream services.
/// </summary>
public class VerifyEmailCommand : IRequest<bool>
{
    /// <summary>
    /// Gets or sets the email verification token sent to the user's email address.
    /// Generated during registration as GUID with 24-hour expiration.
    /// Included in verification email link: /verify-email?token={token}
    /// Handler validates token exists in database and not expired.
    /// Token deleted after successful verification for security.
    /// Required field. Example: "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
