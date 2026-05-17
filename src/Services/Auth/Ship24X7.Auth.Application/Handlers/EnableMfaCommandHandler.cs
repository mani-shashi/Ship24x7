using MediatR;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.DTOs;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Handles MFA enablement by generating TOTP secret, QR code URI, and backup codes.
/// Creates or updates MFA settings for the user. MFA is not fully enabled until user verifies with a valid code.
/// </summary>
public class EnableMfaCommandHandler : IRequestHandler<EnableMfaCommand, MfaSetupResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IMfaService _mfaService;

    /// <summary>
    /// Initializes a new instance of the EnableMfaCommandHandler class.
    /// </summary>
    /// <param name="userRepository">Repository for user data operations.</param>
    /// <param name="mfaService">Service for MFA operations including TOTP generation.</param>
    public EnableMfaCommandHandler(IUserRepository userRepository, IMfaService mfaService)
    {
        _userRepository = userRepository;
        _mfaService = mfaService;
    }

    /// <summary>
    /// Generates MFA setup information including TOTP secret, QR code URI, and backup codes for user enrollment.
    /// Process flow:
    /// 1. Retrieves user by ID from database
    /// 2. Generates new TOTP secret (20-byte random key, base32-encoded)
    /// 3. Generates QR code URI in otpauth:// format for authenticator apps (Google Authenticator, Authy, etc.)
    /// 4. Generates 10 backup codes (8-character alphanumeric) for account recovery
    /// 5. Creates new MfaSettings entity if doesn't exist, or updates existing one
    /// 6. Sets IsEnabled=false (will be enabled after user verifies with valid code)
    /// 7. Stores TOTP secret and backup codes, resets FailedAttempts to 0
    /// 8. Persists MFA settings to database
    /// 9. Returns setup response with secret, QR code URI, and backup codes for client display
    /// User must scan QR code with authenticator app and verify with generated code to complete MFA setup.
    /// </summary>
    /// <param name="request">The enable MFA command containing user ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>MFA setup response with TOTP secret, QR code URI (otpauth://), and 10 backup codes.</returns>
    /// <exception cref="InvalidOperationException">Thrown when user is not found in the database.</exception>
    public async Task<MfaSetupResponse> Handle(EnableMfaCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Generate TOTP secret
        var secret = _mfaService.GenerateTotpSecret();
        var qrCodeUri = _mfaService.GenerateQrCodeUri(user.Email, secret);
        var backupCodes = _mfaService.GenerateBackupCodes();

        // Create or update MFA settings
        if (user.MfaSettings == null)
        {
            user.MfaSettings = new MfaSettings
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                IsEnabled = false, // Will be enabled after verification
                TotpSecret = secret,
                BackupCodes = backupCodes,
                FailedAttempts = 0,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.Id
            };
        }
        else
        {
            user.MfaSettings.TotpSecret = secret;
            user.MfaSettings.BackupCodes = backupCodes;
            user.MfaSettings.IsEnabled = false;
        }

        await _userRepository.UpdateAsync(user);

        return new MfaSetupResponse
        {
            Secret = secret,
            QrCodeUri = qrCodeUri,
            BackupCodes = backupCodes
        };
    }
}
