using MediatR;
using Microsoft.Extensions.Logging;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Handles forgot password requests by generating reset tokens and sending reset emails.
/// Always returns success to prevent email enumeration attacks (security best practice).
/// </summary>
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IVerificationTokenRepository _verificationTokenRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the ForgotPasswordCommandHandler class.
    /// </summary>
    /// <param name="userRepository">Repository for user data operations.</param>
    /// <param name="verificationTokenRepository">Repository for verification token operations.</param>
    /// <param name="emailService">Service for sending password reset emails.</param>
    /// <param name="logger">Logger for recording password reset operations.</param>
    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IVerificationTokenRepository verificationTokenRepository,
        IEmailService emailService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userRepository = userRepository;
        _verificationTokenRepository = verificationTokenRepository;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Processes forgot password request and sends reset email if user exists.
    /// Process flow:
    /// 1. Searches for user by email address
    /// 2. If user not found, logs attempt and returns success (prevents email enumeration)
    /// 3. If user found, generates unique reset token (GUID)
    /// 4. Creates VerificationToken entity with TokenType=PasswordReset and 1-hour expiration
    /// 5. Saves token to database
    /// 6. Sends password reset email with token link to user's email
    /// 7. Returns success
    /// Security: Always returns true even if email not found to prevent attackers from discovering valid emails.
    /// </summary>
    /// <param name="request">Command containing email address for password reset.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Always returns true for security (prevents email enumeration).</returns>
    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find user by email
            var user = await _userRepository.GetByEmailAsync(request.Email);
            
            // If user not found, return success to prevent email enumeration
            if (user == null)
            {
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
                return true;
            }

            // Generate reset token
            var resetToken = Guid.NewGuid().ToString("N");

            // Create verification token entity
            var verificationToken = new VerificationToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = resetToken,
                Type = VerificationTokenType.PasswordReset,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.Id
            };

            // Save token to database
            await _verificationTokenRepository.AddAsync(verificationToken);

            // Send password reset email
            await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, resetToken);

            _logger.LogInformation("Password reset email sent to user: {UserId}", user.Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request for email: {Email}", request.Email);
            // Return true even on error to prevent information leakage
            return true;
        }
    }
}
