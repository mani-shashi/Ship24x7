using MediatR;

namespace Ship24X7.Auth.Application.Commands;

/// <summary>
/// Command for disabling multi-factor authentication (MFA) for a user account.
/// Encapsulates user ID, password, and MFA code for security verification before disabling.
/// Implements IRequest pattern from MediatR for CQRS architecture.
/// Returns boolean indicating successful MFA disabling (true) or failure (false).
/// Requires both password verification and valid MFA code to prevent unauthorized MFA disabling.
/// Updates MFA settings IsEnabled=false and optionally deletes secret key for security.
/// User can re-enable MFA later by going through setup process again with new secret key.
/// </summary>
public class DisableMfaCommand : IRequest<bool>
{
    /// <summary>
    /// Gets or sets the unique identifier of the user disabling MFA.
    /// Extracted from JWT token claims by controller.
    /// Used to retrieve user account and MFA settings for verification and update.
    /// Required field. Example: "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the user's password for identity verification.
    /// Compared against stored BCrypt hash to ensure user owns the account.
    /// Prevents unauthorized MFA disabling if account is compromised.
    /// Required field. Example: "SecureP@ss123"
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the current MFA code to verify before disabling.
    /// 6-digit TOTP code from authenticator app.
    /// Ensures user has access to authenticator device before disabling MFA.
    /// Provides additional security layer beyond password verification.
    /// Required field. Example: "123456"
    /// </summary>
    public string MfaCode { get; set; } = string.Empty;
}
