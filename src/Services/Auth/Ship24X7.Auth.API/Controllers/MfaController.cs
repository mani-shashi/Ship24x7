using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ship24X7.Auth.Application.Commands;
using System.Security.Claims;

namespace Ship24X7.Auth.API.Controllers;

/// <summary>
/// API controller for managing multi-factor authentication (MFA) operations in the Ship24X7 auth service.
/// Handles HTTP requests for setting up, verifying, and disabling TOTP-based two-factor authentication.
/// Provides enhanced account security by requiring a second authentication factor (time-based one-time password).
/// All endpoints require authentication via JWT token.
/// Workflow: Setup MFA -> Scan QR code -> Verify code -> MFA enabled -> Login with MFA -> Disable MFA (optional).
/// Uses TOTP (Time-based One-Time Password) algorithm compatible with Google Authenticator, Authy, and similar apps.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class MfaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<MfaController> _logger;

    /// <summary>
    /// Initializes a new instance of the MfaController class.
    /// Sets up dependencies for command processing and logging.
    /// </summary>
    /// <param name="mediator">MediatR instance for sending commands to their respective handlers using CQRS pattern</param>
    /// <param name="logger">Logger instance for recording MFA operations, security events, errors, and diagnostic information</param>
    public MfaController(IMediator mediator, ILogger<MfaController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Initiates MFA setup for the authenticated user by generating TOTP secret and QR code.
    /// Endpoint: POST /api/v1/mfa/setup
    /// Logic flow:
    /// 1. Extracts user ID from JWT token claims
    /// 2. Creates EnableMfaCommand with user ID
    /// 3. Sends command to handler via MediatR for processing
    /// 4. Handler retrieves user from database by user ID
    /// 5. If user not found, returns error
    /// 6. Handler generates random 32-character base32-encoded secret key for TOTP
    /// 7. Handler creates MfaSettings entity with secret key, IsEnabled=false, and saves to database
    /// 8. Handler generates QR code URL using format: otpauth://totp/Ship24X7:{email}?secret={secret}&issuer=Ship24X7
    /// 9. Handler generates QR code image as base64-encoded PNG
    /// 10. Returns MfaSetupResponse with secret key (for manual entry) and QR code URL
    /// User must scan QR code with authenticator app (Google Authenticator, Authy, etc.) to complete setup.
    /// MFA not enabled until user verifies code in next step.
    /// Used by frontend MFA setup page to display QR code and secret key.
    /// </summary>
    /// <returns>
    /// 200 OK with MfaSetupResponse containing secret key and QR code URL for scanning.
    /// 500 Internal Server Error for unexpected errors during MFA setup.
    /// </returns>
    [HttpPost("setup")]
    public async Task<IActionResult> Setup()
    {
        try
        {
            var userId = GetUserId();
            var command = new EnableMfaCommand { UserId = userId };
            var response = await _mediator.Send(command);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during MFA setup");
            return StatusCode(500, new { error = "An error occurred during MFA setup" });
        }
    }

    /// <summary>
    /// Verifies the MFA code provided by the user and enables MFA for their account.
    /// Endpoint: POST /api/v1/mfa/verify
    /// Logic flow:
    /// 1. Extracts user ID from JWT token claims
    /// 2. Sets user ID in VerifyMfaCommand
    /// 3. Receives MFA code from request body (6-digit TOTP code)
    /// 4. Sends command to handler via MediatR for processing
    /// 5. Handler retrieves MfaSettings from database by user ID
    /// 6. If MFA settings not found, returns false (setup not initiated)
    /// 7. Handler validates TOTP code using secret key and current timestamp
    /// 8. TOTP algorithm: HMAC-SHA1(secret, time_step) where time_step = floor(current_time / 30 seconds)
    /// 9. Handler allows time window of ±1 step (90 seconds total) to account for clock drift
    /// 10. If code invalid, returns 400 Bad Request with "Invalid MFA code" message
    /// 11. If code valid, updates MfaSettings IsEnabled=true and saves to database
    /// 12. Handler publishes MfaEnabled domain event for downstream services
    /// 13. Returns success message
    /// User must provide valid code from authenticator app to complete MFA setup.
    /// After enabling, user must provide MFA code during login.
    /// </summary>
    /// <param name="command">Command containing 6-digit MFA code from authenticator app</param>
    /// <returns>
    /// 200 OK with success message if MFA code valid and MFA enabled successfully.
    /// 400 Bad Request if MFA code invalid or MFA setup not initiated.
    /// 500 Internal Server Error for unexpected errors during MFA verification.
    /// </returns>
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] VerifyMfaCommand command)
    {
        try
        {
            command.UserId = GetUserId();
            var result = await _mediator.Send(command);

            if (result)
            {
                return Ok(new { message = "MFA enabled successfully" });
            }

            return BadRequest(new { error = "Invalid MFA code" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during MFA verification");
            return StatusCode(500, new { error = "An error occurred during MFA verification" });
        }
    }

    /// <summary>
    /// Disables MFA for the authenticated user's account.
    /// Endpoint: POST /api/v1/mfa/disable
    /// Logic flow:
    /// 1. Extracts user ID from JWT token claims
    /// 2. Sets user ID in DisableMfaCommand
    /// 3. Receives user's password from request body for security verification
    /// 4. Sends command to handler via MediatR for processing
    /// 5. Handler retrieves user from database by user ID
    /// 6. If user not found, returns error
    /// 7. Handler verifies password hash using BCrypt comparison
    /// 8. If password incorrect, returns 401 Unauthorized with "Invalid password" message
    /// 9. Handler retrieves MfaSettings from database by user ID
    /// 10. If MFA settings not found or already disabled, returns success
    /// 11. Handler updates MfaSettings IsEnabled=false and saves to database
    /// 12. Handler optionally deletes secret key for security
    /// 13. Returns success message
    /// Requires password verification to prevent unauthorized MFA disabling.
    /// User can re-enable MFA later by going through setup process again.
    /// Used by frontend account security settings to disable MFA.
    /// </summary>
    /// <param name="command">Command containing user's password for security verification</param>
    /// <returns>
    /// 200 OK with success message if MFA disabled successfully.
    /// 400 Bad Request if operation fails.
    /// 401 Unauthorized if password incorrect.
    /// 500 Internal Server Error for unexpected errors during MFA disable.
    /// </returns>
    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromBody] DisableMfaCommand command)
    {
        try
        {
            command.UserId = GetUserId();
            var result = await _mediator.Send(command);

            if (result)
            {
                return Ok(new { message = "MFA disabled successfully" });
            }

            return BadRequest(new { error = "Failed to disable MFA" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during MFA disable");
            return StatusCode(500, new { error = "An error occurred while disabling MFA" });
        }
    }

    /// <summary>
    /// Extracts the user ID from the JWT token claims.
    /// Helper method used by all MFA endpoints to identify the authenticated user.
    /// Logic flow:
    /// 1. Retrieves NameIdentifier claim from JWT token (contains user ID)
    /// 2. If claim missing or empty, throws UnauthorizedAccessException
    /// 3. Parses claim value as GUID
    /// 4. If parsing fails, throws UnauthorizedAccessException
    /// 5. Returns user ID as GUID
    /// Used internally by controller methods to get current user ID.
    /// </summary>
    /// <returns>The authenticated user's unique identifier (GUID)</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if user ID claim missing, empty, or invalid in JWT token</exception>
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }
        return userId;
    }
}
