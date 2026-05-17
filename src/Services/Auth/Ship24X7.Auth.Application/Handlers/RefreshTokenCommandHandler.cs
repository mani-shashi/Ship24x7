using MediatR;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.DTOs;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Handles refresh token validation and generation of new access/refresh token pairs.
/// Implements token rotation for security and detects token replay attacks.
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;

    /// <summary>
    /// Initializes a new instance of the RefreshTokenCommandHandler class.
    /// </summary>
    /// <param name="userRepository">Repository for user data operations.</param>
    /// <param name="refreshTokenRepository">Repository for refresh token operations.</param>
    /// <param name="tokenService">Service for generating JWT tokens.</param>
    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Validates the refresh token and generates new access and refresh tokens using token rotation.
    /// Process flow:
    /// 1. Retrieves refresh token from database
    /// 2. Validates token exists, not revoked, and not expired
    /// 3. Checks for replay attack (token already used - ReplacedByToken not null)
    /// 4. If replay detected, revokes entire token family to prevent compromise
    /// 5. Retrieves user with roles and validates user is active
    /// 6. Generates new access token (15-minute expiry) with user roles and claims
    /// 7. Generates new refresh token (7-day expiry)
    /// 8. Revokes old refresh token and links to new token via ReplacedByToken
    /// 9. Stores new refresh token with same TokenFamily for lineage tracking
    /// 10. Returns new token pair to client
    /// Token rotation ensures each refresh token is single-use, preventing token theft exploitation.
    /// </summary>
    /// <param name="request">The refresh token command containing the current refresh token.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>New login response with fresh access token (15-min expiry) and refresh token (7-day expiry).</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when token is invalid, expired, revoked, replay detected, or user inactive.</exception>
    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Get refresh token
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);
        if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        // Check for replay attack
        if (refreshToken.ReplacedByToken != null)
        {
            // Token has been used before - revoke entire token family
            await _refreshTokenRepository.RevokeTokenFamilyAsync(refreshToken.TokenFamily);
            throw new UnauthorizedAccessException("Token replay detected");
        }

        // Get user with roles
        var user = await _userRepository.GetByEmailWithRolesAsync(refreshToken.User.Email);
        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("User not found or inactive");
        }

        // Get user roles and claims
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var claims = user.UserClaims.Select(uc => uc.ClaimValue).ToList();

        // Generate new tokens
        var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, roles, claims);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Revoke old refresh token
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.ReplacedByToken = newRefreshToken;
        await _refreshTokenRepository.UpdateAsync(refreshToken);

        // Store new refresh token
        var newRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            TokenFamily = refreshToken.TokenFamily,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Id
        };

        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);

        // Build user response
        var userResponse = new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            EmailVerified = user.EmailVerified,
            IsActive = user.IsActive,
            Roles = roles,
            MfaEnabled = user.MfaSettings?.IsEnabled ?? false,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };

        return new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            RequiresMfa = false,
            User = userResponse
        };
    }
}
