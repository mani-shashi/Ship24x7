using MediatR;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.Interfaces;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Handles MFA code verification during the MFA setup process.
/// Validates TOTP code against user's secret and enables MFA if verification succeeds.
/// </summary>
public class VerifyMfaCommandHandler : IRequestHandler<VerifyMfaCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IMfaService _mfaService;

    /// <summary>
    /// Initializes a new instance of the VerifyMfaCommandHandler class.
    /// </summary>
    /// <param name="userRepository">Repository for user data operations.</param>
    /// <param name="mfaService">Service for TOTP code validation.</param>
    public VerifyMfaCommandHandler(IUserRepository userRepository, IMfaService mfaService)
    {
        _userRepository = userRepository;
        _mfaService = mfaService;
    }

    /// <summary>
    /// Verifies the TOTP code and enables MFA if valid, completing the MFA setup process.
    /// Process flow:
    /// 1. Retrieves user with MFA settings from database
    /// 2. Validates user and MFA settings exist
    /// 3. Validates TOTP code against user's TOTP secret using time-based algorithm
    /// 4. Allows codes from 2 time steps before and after current time (handles clock drift)
    /// 5. If code is valid:
    ///    a. Sets MfaSettings.IsEnabled = true
    ///    b. Persists changes to database
    ///    c. Returns true
    /// 6. If code is invalid, returns false without enabling MFA
    /// This completes the MFA enrollment process started by EnableMfaCommand.
    /// </summary>
    /// <param name="request">The verify MFA command with user ID and 6-digit TOTP code.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if code is valid and MFA was enabled successfully; false if code is invalid.</returns>
    /// <exception cref="InvalidOperationException">Thrown when user or MFA settings not found in database.</exception>
    public async Task<bool> Handle(VerifyMfaCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null || user.MfaSettings == null)
        {
            throw new InvalidOperationException("User or MFA settings not found");
        }

        // Validate TOTP code
        if (!_mfaService.ValidateTotpCode(user.MfaSettings.TotpSecret, request.Code))
        {
            return false;
        }

        // Enable MFA
        user.MfaSettings.IsEnabled = true;
        await _userRepository.UpdateAsync(user);

        return true;
    }
}
