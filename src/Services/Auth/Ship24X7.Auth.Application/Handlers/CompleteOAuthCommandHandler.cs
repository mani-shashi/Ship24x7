using MediatR;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.DTOs;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Handles OAuth flow completion by exchanging authorization code for user profile.
/// Creates new user if not exists, stores external login information, and generates authentication tokens.
/// </summary>
public class CompleteOAuthCommandHandler : IRequestHandler<CompleteOAuthCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IOAuthService _oauthService;
    private readonly ITokenService _tokenService;

    /// <summary>
    /// Initializes a new instance of the CompleteOAuthCommandHandler class.
    /// </summary>
    /// <param name="userRepository">Repository for user data operations.</param>
    /// <param name="roleRepository">Repository for role data operations.</param>
    /// <param name="refreshTokenRepository">Repository for refresh token operations.</param>
    /// <param name="oauthService">Service for OAuth provider operations.</param>
    /// <param name="tokenService">Service for generating JWT tokens.</param>
    public CompleteOAuthCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IOAuthService oauthService,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _oauthService = oauthService;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Completes OAuth authentication by exchanging authorization code for user profile and creating/authenticating user.
    /// Process flow:
    /// 1. Exchanges authorization code with OAuth provider for access token
    /// 2. Retrieves user profile (email, name, provider ID) from OAuth provider
    /// 3. Searches for existing user by email
    /// 4. If user doesn't exist:
    ///    a. Creates new user with email (lowercase), full name from OAuth profile
    ///    b. Sets EmailVerified=true (OAuth provider verified), PasswordHash=empty (no password for OAuth users)
    ///    c. Sets IsActive=true, PhoneVerified=false
    ///    d. Assigns default "Customer" role
    /// 5. Stores or updates ExternalLogin with provider name, provider ID, access token, refresh token, and expiry
    /// 6. Updates user's LastLoginAt timestamp
    /// 7. Retrieves user roles and claims for token generation
    /// 8. Generates access token (15-minute expiry) and refresh token (7-day expiry)
    /// 9. Stores refresh token with new token family ID
    /// 10. Returns login response with tokens
    /// OAuth users bypass password requirements and have email automatically verified.
    /// </summary>
    /// <param name="request">The complete OAuth command with provider, authorization code, redirect URI, and state.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Login response with access token, refresh token, and expiration timestamp.</returns>
    public async Task<LoginResponse> Handle(CompleteOAuthCommand request, CancellationToken cancellationToken)
    {
        // Exchange code for user profile
        var profile = await _oauthService.ExchangeCodeForProfileAsync(
            request.Provider,
            request.Code,
            request.RedirectUri);

        // Find or create user
        var user = await _userRepository.GetByEmailWithRolesAsync(profile.Email);
        if (user == null)
        {
            // Create new user
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = profile.Email.ToLowerInvariant(),
                PasswordHash = string.Empty, // No password for OAuth users
                FullName = profile.FullName,
                PhoneNumber = string.Empty,
                EmailVerified = true, // OAuth provider has verified email
                PhoneVerified = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            };

            await _userRepository.AddAsync(user);

            // Assign Customer role
            var customerRole = await _roleRepository.GetByNameAsync("Customer");
            if (customerRole != null)
            {
                user.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RoleId = customerRole.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = Guid.Empty
                });
                await _userRepository.UpdateAsync(user);
            }
        }

        // Store or update external login
        var externalLogin = user.ExternalLogins.FirstOrDefault(el => el.Provider == request.Provider);
        if (externalLogin == null)
        {
            externalLogin = new ExternalLogin
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Provider = request.Provider,
                ProviderKey = profile.ProviderId,
                ProviderDisplayName = request.Provider,
                AccessToken = profile.AccessToken,
                RefreshToken = profile.RefreshToken,
                TokenExpiresAt = profile.TokenExpiresAt,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.Id
            };
            user.ExternalLogins.Add(externalLogin);
        }
        else
        {
            externalLogin.AccessToken = profile.AccessToken;
            externalLogin.RefreshToken = profile.RefreshToken;
            externalLogin.TokenExpiresAt = profile.TokenExpiresAt;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Get user roles and claims
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var claims = user.UserClaims.Select(uc => uc.ClaimValue).ToList();

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, roles, claims);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Store refresh token
        var tokenFamily = Guid.NewGuid().ToString("N");
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            TokenFamily = tokenFamily,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Id
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            RequiresMfa = false
        };
    }
}
