using MediatR;
using Ship24X7.Auth.Application.DTOs;

namespace Ship24X7.Auth.Application.Commands;

/// <summary>
/// Command for refreshing an expired JWT access token using a valid refresh token.
/// Encapsulates refresh token for validation and new token generation.
/// Implements IRequest pattern from MediatR for CQRS architecture.
/// Returns LoginResponse containing new access token, new refresh token, and expiration.
/// Implements token rotation security pattern: old refresh token revoked, new refresh token issued.
/// Validates refresh token is not expired, not revoked, and belongs to active user.
/// Used to maintain user session without requiring re-authentication when access token expires.
/// </summary>
public class RefreshTokenCommand : IRequest<LoginResponse>
{
    /// <summary>
    /// Gets or sets the refresh token to validate and use for generating new tokens.
    /// Retrieved from HttpOnly cookie by controller for security.
    /// Must be valid (not expired, not revoked) and belong to an active user account.
    /// Handler validates token exists in database and checks expiration (7-day lifetime).
    /// After successful validation, old token is revoked and new token pair is issued.
    /// Required field. Example: "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
