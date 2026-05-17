using MediatR;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.Interfaces;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Handles user logout by revoking the refresh token to invalidate the session.
/// Prevents further use of the token for authentication and token refresh operations.
/// </summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    /// <summary>
    /// Initializes a new instance of the LogoutCommandHandler class.
    /// </summary>
    /// <param name="refreshTokenRepository">Repository for refresh token operations.</param>
    public LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    /// <summary>
    /// Revokes the refresh token to log out the user and invalidate their session.
    /// Process flow:
    /// 1. Retrieves refresh token from database by token string
    /// 2. If token not found, returns false (already logged out or invalid token)
    /// 3. If token found:
    ///    a. Sets IsRevoked = true to mark token as invalid
    ///    b. Sets RevokedAt = current UTC timestamp for audit trail
    ///    c. Persists changes to database
    ///    d. Returns true
    /// Access tokens remain valid until expiry (15 minutes), but refresh token cannot be used to obtain new access tokens.
    /// For immediate session termination, client should discard access token and redirect to login.
    /// </summary>
    /// <param name="request">The logout command containing the refresh token to revoke.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if token was revoked successfully; false if token not found.</returns>
    public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
        if (refreshToken == null)
        {
            return false;
        }

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(refreshToken);

        return true;
    }
}
