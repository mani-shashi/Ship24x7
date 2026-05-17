using MediatR;

namespace Ship24X7.Auth.Application.Commands;

/// <summary>
/// Command for logging out a user by invalidating their refresh token.
/// Encapsulates refresh token for revocation during logout process.
/// Implements IRequest pattern from MediatR for CQRS architecture.
/// Returns boolean indicating successful logout (true) or failure (false).
/// Revokes refresh token in database to prevent further token refresh operations.
/// Access token remains valid until expiration (15 minutes) but cannot be refreshed.
/// Controller also deletes refresh token cookie for complete session termination.
/// </summary>
public class LogoutCommand : IRequest<bool>
{
    /// <summary>
    /// Gets or sets the refresh token to revoke during logout.
    /// Retrieved from HttpOnly cookie by controller.
    /// Handler marks token as revoked (IsRevoked=true) in database.
    /// If token not found in database, logout still succeeds (idempotent operation).
    /// Required field. Example: "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
