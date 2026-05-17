using MediatR;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.DTOs;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Handles user login authentication with password verification, account validation, and MFA support.
/// Implements account lockout after failed attempts and generates JWT tokens upon successful authentication.
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IMfaService _mfaService;

    /// <summary>
    /// Initializes a new instance of the LoginCommandHandler class.
    /// </summary>
    /// <param name="userRepository">Repository for user data operations.</param>
    /// <param name="refreshTokenRepository">Repository for refresh token operations.</param>
    /// <param name="passwordHasher">Service for password verification.</param>
    /// <param name="tokenService">Service for generating JWT tokens.</param>
    /// <param name="mfaService">Service for MFA code validation.</param>
    public LoginCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IMfaService mfaService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _mfaService = mfaService;
    }

    /// <summary>
    /// Authenticates user credentials and generates authentication tokens.
    /// Process flow:
    /// 1. Retrieves user by email with roles
    /// 2. Validates account is not locked (15-minute lockout after 5 failed attempts)
    /// 3. Verifies password hash
    /// 4. Checks email verification status
    /// 5. Validates account is active
    /// 6. If MFA enabled, validates TOTP code (3 failed attempts = 5-minute lockout)
    /// 7. Resets failed login attempts on success
    /// 8. Updates last login timestamp
    /// 9. Generates access token (15-minute expiry) and refresh token (7-day expiry)
    /// 10. Stores refresh token with family ID for rotation tracking
    /// </summary>
    /// <param name="request">Login command containing email, password, and optional MFA code.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Login response with tokens if successful, or RequiresMfa flag if MFA code needed.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when credentials invalid, account locked, email not verified, or account inactive.</exception>
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Get user with roles
        var user = await _userRepository.GetByEmailWithRolesAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        // Check if account is locked
        if (user.IsLocked && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Account is locked");
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            // Increment failed login attempts
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.IsLocked = true;
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            }
            await _userRepository.UpdateAsync(user);
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        // Check if email is verified
        if (!user.EmailVerified)
        {
            throw new UnauthorizedAccessException("Email not verified");
        }

        // Check if account is active
        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is not active");
        }

        // Check if MFA is enabled
        if (user.MfaSettings?.IsEnabled == true)
        {
            // Check if MFA is locked
            if (user.MfaSettings.LockedUntil.HasValue && user.MfaSettings.LockedUntil.Value > DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("MFA is temporarily locked");
            }

            // If MFA code not provided, return response indicating MFA is required
            if (string.IsNullOrEmpty(request.MfaCode))
            {
                return new LoginResponse
                {
                    RequiresMfa = true,
                    AccessToken = null,
                    RefreshToken = null
                };
            }

            // Validate MFA code
            if (!_mfaService.ValidateTotpCode(user.MfaSettings.TotpSecret, request.MfaCode))
            {
                // Increment failed MFA attempts
                user.MfaSettings.FailedAttempts++;
                if (user.MfaSettings.FailedAttempts >= 3)
                {
                    user.MfaSettings.LockedUntil = DateTime.UtcNow.AddMinutes(5);
                }
                await _userRepository.UpdateAsync(user);
                throw new UnauthorizedAccessException("Invalid MFA code");
            }

            // Reset MFA failed attempts
            user.MfaSettings.FailedAttempts = 0;
            user.MfaSettings.LockedUntil = null;
        }

        // Reset failed login attempts
        user.FailedLoginAttempts = 0;
        user.IsLocked = false;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Get user roles
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
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            RequiresMfa = false,
            User = userResponse
        };
    }
}
