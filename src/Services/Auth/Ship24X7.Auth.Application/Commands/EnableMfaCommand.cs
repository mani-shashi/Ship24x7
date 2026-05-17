using MediatR;
using Ship24X7.Auth.Application.DTOs;

namespace Ship24X7.Auth.Application.Commands;

/// <summary>
/// Command for initiating multi-factor authentication (MFA) setup for a user account.
/// Encapsulates user ID for MFA configuration.
/// Implements IRequest pattern from MediatR for CQRS architecture.
/// Returns MfaSetupResponse containing TOTP secret key and QR code for authenticator app setup.
/// Generates random 32-character base32-encoded secret key for TOTP algorithm.
/// Creates QR code URL and image for easy scanning with Google Authenticator, Authy, or similar apps.
/// MFA not enabled until user verifies code in subsequent step.
/// </summary>
public class EnableMfaCommand : IRequest<MfaSetupResponse>
{
    /// <summary>
    /// Gets or sets the unique identifier of the user enabling MFA.
    /// Extracted from JWT token claims by controller.
    /// Used to retrieve user account and create/update MFA settings.
    /// Required field. Example: "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
    /// </summary>
    public Guid UserId { get; set; }
}
