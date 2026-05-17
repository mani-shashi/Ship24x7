using MediatR;
using Microsoft.Extensions.Logging;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Handles password reset requests by validating reset tokens and updating user passwords.
/// Revokes all refresh tokens after password reset to force re-login on all devices.
/// </summary>
public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IVerificationTokenRepository _verificationTokenRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the ResetPasswordCommandHandler class.
    /// </summary>
    /// <param name="userRepository">Repository for user data operations.</param>
    /// <param name="verificationTokenRepository">Repository for verification token operations.</param>
    /// <param name="refreshTokenRepository">Repository for refresh token operations.</param>
    /// <param name="passwordHasher">Service for password hashing.</param>
    /// <param name="logger">Logger for recording password reset operations.</param>
    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IVerificationTokenRepository verificationTokenRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _verificationTokenRepository = verificationTokenRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    /// <summary>
    /// Processes password reset request and updates user password if token is valid.
    /// Process flow:
    /// 1. Retrieves verification token from database by token value
    /// 2. Validates token exists and type is PasswordReset
    /// 3. Validates token is not expired (ExpiresAt > current time)
    /// 4. Validates token is not already used (IsUsed = false)
    /// 5. Retrieves user from database by user ID from token
    /// 6. Validates user exists and account is active
    /// 7. Hashes new password using BCrypt
    /// 8. Updates user password hash in database
    /// 9. Marks verification token as used (IsUsed=true, UsedAt=current time)
    /// 10. Revokes all refresh tokens for user (forces re-login on all devices)
    /// 11. Returns success
    /// Security: Token is single-use and expires in 1 hour. All sessions invalidated after reset.
    /// </summary>
    /// <param name="request">Command containing reset token and new password.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if password reset succeeds.</returns>
    /// <exception cref="InvalidOperationException">Thrown when token invalid, expired, or user not found.</exception>
    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Retrieve verification token
        var verificationToken = await _verificationTokenRepository.GetByTokenAsync(request.Token);
        
        if (verificationToken == null || verificationToken.Type != VerificationTokenType.PasswordReset)
        {
            throw new InvalidOperationException("Invalid password reset token");
        }

        // Check if token is expired
        if (verificationToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Password reset token has expired");
        }

        // Check if token is already used
        if (verificationToken.IsUsed)
        {
            throw new InvalidOperationException("Password reset token has already been used");
        }

        // Retrieve user
        var user = await _userRepository.GetByIdAsync(verificationToken.UserId);
        
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("Account is not active");
        }

        // Hash new password
        var passwordHash = _passwordHasher.HashPassword(request.NewPassword);

        // Update user password
        user.PasswordHash = passwordHash;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = user.Id;
        await _userRepository.UpdateAsync(user);

        // Mark token as used
        verificationToken.IsUsed = true;
        verificationToken.UsedAt = DateTime.UtcNow;
        verificationToken.UpdatedAt = DateTime.UtcNow;
        verificationToken.UpdatedBy = user.Id;
        await _verificationTokenRepository.UpdateAsync(verificationToken);

        // Revoke all refresh tokens for user (force re-login on all devices)
        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id);

        _logger.LogInformation("Password reset successful for user: {UserId}", user.Id);

        return true;
    }
}
