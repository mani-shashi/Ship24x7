using MediatR;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.Interfaces;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Handles MFA disablement by verifying password and MFA code before removing MFA settings.
/// Requires both password and valid MFA code for security to prevent unauthorized MFA removal.
/// </summary>
public class DisableMfaCommandHandler : IRequestHandler<DisableMfaCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMfaService _mfaService;

    /// <summary>
    /// Initializes a new instance of the DisableMfaCommandHandler class.
    /// </summary>
    /// <param name="userRepository">Repository for user data operations.</param>
    /// <param name="passwordHasher">Service for password verification.</param>
    /// <param name="mfaService">Service for MFA code validation.</param>
    public DisableMfaCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IMfaService mfaService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _mfaService = mfaService;
    }

    /// <summary>
    /// Disables MFA for the user after verifying password and MFA code for security.
    /// Process flow:
    /// 1. Retrieves user with MFA settings from database
    /// 2. Validates user and MFA settings exist
    /// 3. Verifies user's password using secure hash comparison
    /// 4. Validates current MFA code against TOTP secret
    /// 5. If both password and MFA code are valid:
    ///    a. Sets MfaSettings.IsEnabled = false
    ///    b. Clears TOTP secret (sets to empty string)
    ///    c. Clears backup codes (sets to empty array)
    ///    d. Persists changes to database
    ///    e. Returns true
    /// 6. If password or MFA code invalid, throws UnauthorizedAccessException
    /// Dual verification (password + MFA code) prevents unauthorized MFA removal by attackers with stolen session tokens.
    /// </summary>
    /// <param name="request">The disable MFA command with user ID, password, and current MFA code.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if MFA was disabled successfully.</returns>
    /// <exception cref="InvalidOperationException">Thrown when user or MFA settings not found in database.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when password or MFA code is invalid.</exception>
    public async Task<bool> Handle(DisableMfaCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null || user.MfaSettings == null)
        {
            throw new InvalidOperationException("User or MFA settings not found");
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid password");
        }

        // Verify MFA code
        if (!_mfaService.ValidateTotpCode(user.MfaSettings.TotpSecret, request.MfaCode))
        {
            throw new UnauthorizedAccessException("Invalid MFA code");
        }

        // Disable MFA
        user.MfaSettings.IsEnabled = false;
        user.MfaSettings.TotpSecret = string.Empty;
        user.MfaSettings.BackupCodes = Array.Empty<string>();
        await _userRepository.UpdateAsync(user);

        return true;
    }
}
