using MediatR;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Handles email verification by validating the verification token and marking email as verified.
/// Ensures token is valid, not expired, not already used, and of correct type before verification.
/// </summary>
public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IVerificationTokenRepository _verificationTokenRepository;

    /// <summary>
    /// Initializes a new instance of the VerifyEmailCommandHandler class.
    /// </summary>
    /// <param name="userRepository">Repository for user data operations.</param>
    /// <param name="verificationTokenRepository">Repository for verification token operations.</param>
    public VerifyEmailCommandHandler(
        IUserRepository userRepository,
        IVerificationTokenRepository verificationTokenRepository)
    {
        _userRepository = userRepository;
        _verificationTokenRepository = verificationTokenRepository;
    }

    /// <summary>
    /// Verifies the user's email address using the provided verification token.
    /// Process flow:
    /// 1. Retrieves verification token from database by token string
    /// 2. Validates token exists (throws if null)
    /// 3. Checks token not already used (IsUsed = false)
    /// 4. Validates token not expired (ExpiresAt > current UTC time)
    /// 5. Verifies token type is EmailVerification (not PasswordReset or PhoneVerification)
    /// 6. Retrieves user by token's UserId
    /// 7. Validates user exists
    /// 8. Checks email not already verified (prevents duplicate verification)
    /// 9. If all validations pass:
    ///    a. Sets User.EmailVerified = true
    ///    b. Updates User.UpdatedAt timestamp
    ///    c. Persists user changes
    ///    d. Marks token as used (IsUsed = true, UsedAt = current UTC time)
    ///    e. Persists token changes
    ///    f. Returns true
    /// Email verification is required before users can log in to the system.
    /// </summary>
    /// <param name="request">The verification command containing the verification token string.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if email was verified successfully.</returns>
    /// <exception cref="InvalidOperationException">Thrown when token is invalid, expired, already used, wrong type, user not found, or email already verified.</exception>
    public async Task<bool> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        // Find verification token
        var verificationToken = await _verificationTokenRepository.GetByTokenAsync(request.Token);
        
        if (verificationToken == null)
        {
            throw new InvalidOperationException("Invalid verification token");
        }

        // Check if token is already used
        if (verificationToken.IsUsed)
        {
            throw new InvalidOperationException("Verification token has already been used");
        }

        // Check if token is expired
        if (verificationToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Verification token has expired");
        }

        // Check if token type is email verification
        if (verificationToken.Type != VerificationTokenType.EmailVerification)
        {
            throw new InvalidOperationException("Invalid token type");
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(verificationToken.UserId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Check if email is already verified
        if (user.EmailVerified)
        {
            throw new InvalidOperationException("Email is already verified");
        }

        // Mark email as verified
        user.EmailVerified = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Mark token as used
        verificationToken.IsUsed = true;
        verificationToken.UsedAt = DateTime.UtcNow;
        await _verificationTokenRepository.UpdateAsync(verificationToken);

        return true;
    }
}
