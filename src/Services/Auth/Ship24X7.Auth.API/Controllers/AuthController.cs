using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.Queries;
using System.Security.Claims;

namespace Ship24X7.Auth.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;

    public AuthController(IMediator mediator, ILogger<AuthController> logger, IConfiguration configuration)
    {
        _mediator = mediator;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Registers a new user account in the Ship24X7 platform.
    /// Endpoint: POST /api/v1/auth/register
    /// Logic flow:
    /// 1. Receives registration request with email, password, full name, phone number, and role
    /// 2. Sends command to handler via MediatR for processing
    /// 3. Handler validates email is not already registered in database
    /// 4. If email exists, returns 400 Bad Request with error message
    /// 5. Handler validates password meets complexity requirements (min 8 chars, uppercase, lowercase, digit, special char)
    /// 6. Handler hashes password using BCrypt with salt rounds for security
    /// 7. Handler creates User entity with IsActive=false, EmailVerified=false, and saves to database
    /// 8. Handler assigns default role (Customer_User) or specified role to user
    /// 9. Handler generates unique email verification token (GUID) with 24-hour expiration
    /// 10. Handler saves verification token to database
    /// 11. Handler sends verification email with token link to user's email address
    /// 12. Handler publishes UserRegistered domain event for downstream services
    /// 13. Returns user ID with success message
    /// User cannot login until email is verified.
    /// Used by frontend registration form to create new customer accounts.
    /// </summary>
    /// <param name="command">Command containing email, password, full name, phone number, and optional role for registration</param>
    /// <returns>
    /// 200 OK with user ID and success message instructing user to check email for verification.
    /// 400 Bad Request if email already exists or validation fails (weak password, invalid email format).
    /// 500 Internal Server Error for unexpected errors during registration.
    /// </returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        try
        {
            var userId = await _mediator.Send(command);
            return Ok(new { userId, message = "Registration successful. Please check your email to verify your account." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return StatusCode(500, new { error = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Authenticates a user and issues JWT access and refresh tokens.
    /// Endpoint: POST /api/v1/auth/login
    /// Logic flow:
    /// 1. Receives login request with email, password, and optional MFA code
    /// 2. Sends command to handler via MediatR for processing
    /// 3. Handler retrieves user from database by email
    /// 4. If user not found, returns 401 Unauthorized with "Invalid credentials" message
    /// 5. Handler verifies password hash using BCrypt comparison
    /// 6. If password incorrect, increments failed login attempts and returns 401 Unauthorized
    /// 7. If failed attempts >= 5, locks account for 30 minutes and returns 401 Unauthorized
    /// 8. Handler checks if email is verified (EmailVerified=true)
    /// 9. If email not verified, returns 401 Unauthorized with "Email not verified" message
    /// 10. Handler checks if account is active (IsActive=true)
    /// 11. If account inactive, returns 401 Unauthorized with "Account deactivated" message
    /// 12. Handler checks if MFA is enabled for user
    /// 13. If MFA enabled and no MFA code provided, returns 200 OK with requiresMfa=true flag
    /// 14. If MFA enabled and MFA code provided, validates TOTP code using secret key
    /// 15. If MFA code invalid, returns 401 Unauthorized with "Invalid MFA code" message
    /// 16. Handler generates JWT access token with user ID, email, roles, and claims (expires in 15 minutes)
    /// 17. Handler generates refresh token (GUID) with 7-day expiration and saves to database
    /// 18. Handler resets failed login attempts to 0
    /// 19. Handler updates last login timestamp
    /// 20. Sets refresh token in HttpOnly, Secure, SameSite=Strict cookie for security
    /// 21. Returns access token and expiration timestamp
    /// Used by frontend login form to authenticate users and obtain access tokens.
    /// Access token used for API authorization, refresh token used to obtain new access tokens.
    /// </summary>
    /// <param name="command">Command containing email, password, and optional MFA code for authentication</param>
    /// <returns>
    /// 200 OK with access token and expiration if login succeeds without MFA.
    /// 200 OK with requiresMfa=true flag if MFA is enabled and code not provided.
    /// 401 Unauthorized if credentials invalid, email not verified, account locked, or MFA code invalid.
    /// 500 Internal Server Error for unexpected errors during login.
    /// </returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        try
        {
            var response = await _mediator.Send(command);
            
            if (response.RequiresMfa)
            {
                return Ok(new { requiresMfa = true, message = "MFA code required" });
            }

            // Set refresh token in HttpOnly cookie
            Response.Cookies.Append("refreshToken", response.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new 
            { 
                accessToken = response.AccessToken,
                expiresAt = response.ExpiresAt,
                user = response.User
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { error = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// Endpoint: POST /api/v1/auth/refresh
    /// Logic flow:
    /// 1. Retrieves refresh token from HttpOnly cookie
    /// 2. If cookie missing, returns 401 Unauthorized with "Refresh token not found" message
    /// 3. Creates RefreshTokenCommand with refresh token
    /// 4. Sends command to handler via MediatR for processing
    /// 5. Handler retrieves refresh token from database by token value
    /// 6. If token not found, returns 401 Unauthorized with "Invalid refresh token" message
    /// 7. Handler checks if token is expired (ExpiresAt < current time)
    /// 8. If expired, deletes token from database and returns 401 Unauthorized
    /// 9. Handler checks if token is revoked (IsRevoked=true)
    /// 10. If revoked, returns 401 Unauthorized with "Token revoked" message
    /// 11. Handler retrieves user from database using user ID from token
    /// 12. If user not found or inactive, returns 401 Unauthorized
    /// 13. Handler generates new JWT access token with user claims (expires in 15 minutes)
    /// 14. Handler generates new refresh token (GUID) with 7-day expiration
    /// 15. Handler revokes old refresh token (sets IsRevoked=true)
    /// 16. Handler saves new refresh token to database
    /// 17. Sets new refresh token in HttpOnly cookie
    /// 18. Returns new access token and expiration timestamp
    /// Used by frontend to obtain new access tokens when current token expires.
    /// Implements token rotation for security (old refresh token invalidated).
    /// </summary>
    /// <returns>
    /// 200 OK with new access token and expiration if refresh succeeds.
    /// 401 Unauthorized if refresh token missing, invalid, expired, or revoked.
    /// 500 Internal Server Error for unexpected errors during token refresh.
    /// </returns>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(new { error = "Refresh token not found" });
            }

            var command = new RefreshTokenCommand { RefreshToken = refreshToken };
            var response = await _mediator.Send(command);

            // Set new refresh token in HttpOnly cookie
            Response.Cookies.Append("refreshToken", response.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new 
            { 
                accessToken = response.AccessToken,
                expiresAt = response.ExpiresAt,
                user = response.User
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { error = "An error occurred during token refresh" });
        }
    }

    /// <summary>
    /// Logs out the current user by revoking their refresh token.
    /// Endpoint: POST /api/v1/auth/logout
    /// Logic flow:
    /// 1. Retrieves refresh token from HttpOnly cookie
    /// 2. If cookie exists, creates LogoutCommand with refresh token
    /// 3. Sends command to handler via MediatR for processing
    /// 4. Handler retrieves refresh token from database by token value
    /// 5. If token found, marks it as revoked (IsRevoked=true) and saves to database
    /// 6. Deletes refresh token cookie from response
    /// 7. Returns success message
    /// Used by frontend logout functionality to invalidate user session.
    /// Access token remains valid until expiration (15 minutes) but cannot be refreshed.
    /// </summary>
    /// <returns>
    /// 200 OK with success message if logout succeeds.
    /// 500 Internal Server Error for unexpected errors during logout.
    /// </returns>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var command = new LogoutCommand { RefreshToken = refreshToken };
                await _mediator.Send(command);
            }

            // Clear refresh token cookie
            Response.Cookies.Delete("refreshToken");

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { error = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// Retrieves the current authenticated user's profile information.
    /// Endpoint: GET /api/v1/auth/me
    /// Logic flow:
    /// 1. Extracts user ID from JWT claims (ClaimTypes.NameIdentifier)
    /// 2. If user ID not found in claims, returns 401 Unauthorized
    /// 3. Creates GetCurrentUserQuery with user ID
    /// 4. Sends query to handler via MediatR for processing
    /// 5. Handler queries database for user with roles and MFA settings
    /// 6. If user not found, handler throws UnauthorizedAccessException
    /// 7. If user is inactive, handler throws UnauthorizedAccessException
    /// 8. Handler maps user entity to UserResponse DTO
    /// 9. Returns complete user profile including:
    ///    - Basic info (id, email, fullName, phoneNumber)
    ///    - Status flags (emailVerified, isActive, mfaEnabled)
    ///    - Roles (list of role names)
    ///    - Timestamps (createdAt, lastLoginAt)
    /// Used by frontend to:
    /// - Display user profile
    /// - Check email verification status
    /// - Show user roles and permissions
    /// - Refresh user data after profile updates
    /// Requires valid JWT token in Authorization header.
    /// </summary>
    /// <returns>
    /// 200 OK with UserResponse containing complete user profile if successful.
    /// 401 Unauthorized if user not authenticated, not found, or account inactive.
    /// 500 Internal Server Error for unexpected errors during profile retrieval.
    /// </returns>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            // Extract user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { error = "Invalid user token" });
            }

            // Query user profile
            var query = new GetCurrentUserQuery { UserId = userId };
            var userProfile = await _mediator.Send(query);

            return Ok(userProfile);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user profile");
            return StatusCode(500, new { error = "An error occurred while retrieving user profile" });
        }
    }

    /// <summary>
    /// Verifies a user's email address using the verification token sent via email.
    /// Endpoint: POST /api/v1/auth/verify-email
    /// Logic flow:
    /// 1. Receives verification request with user ID and verification token
    /// 2. Sends command to handler via MediatR for processing
    /// 3. Handler retrieves verification token from database by user ID and token value
    /// 4. If token not found, returns 400 Bad Request with "Invalid verification token" message
    /// 5. Handler checks if token is expired (ExpiresAt < current time)
    /// 6. If expired, returns 400 Bad Request with "Verification token expired" message
    /// 7. Handler retrieves user from database by user ID
    /// 8. If user not found, returns 400 Bad Request
    /// 9. Handler updates user EmailVerified=true
    /// 10. Handler deletes verification token from database
    /// 11. Handler publishes EmailVerified domain event for downstream services
    /// 12. Returns success message
    /// User can now login after email verification.
    /// Used by frontend after user clicks verification link in email.
    /// </summary>
    /// <param name="command">Command containing user ID and verification token from email link</param>
    /// <returns>
    /// 200 OK with success message if email verification succeeds.
    /// 400 Bad Request if token invalid or expired.
    /// 500 Internal Server Error for unexpected errors during email verification.
    /// </returns>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return Ok(new { message = "Email verified successfully. You can now login." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification");
            return StatusCode(500, new { error = "An error occurred during email verification" });
        }
    }

    /// <summary>
    /// Handles email verification link clicks from emails.
    /// Endpoint: GET /api/v1/auth/verify-email?token=...
    /// Verifies the token then redirects to the frontend — frontend URL is read from
    /// App:FrontendUrl config so port changes never break already-sent emails.
    /// </summary>
    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmailLink([FromQuery] string token)
    {
        var frontendUrl = _configuration["App:FrontendUrl"] ?? "http://localhost:4200";

        try
        {
            var command = new VerifyEmailCommand { Token = token };
            await _mediator.Send(command);

            // Redirect to frontend login page with success flag
            return Redirect($"{frontendUrl}/login?verified=true");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Email verification failed for token: {Error}", ex.Message);
            return Redirect($"{frontendUrl}/login?verified=false&reason={Uri.EscapeDataString(ex.Message)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification link click");
            return Redirect($"{frontendUrl}/login?verified=false");
        }
    }

    /// <summary>
    /// Initiates a password reset request by sending a reset token to the user's email.
    /// Endpoint: POST /api/v1/auth/forgot-password
    /// Logic flow:
    /// 1. Receives forgot password request with email address
    /// 2. Sends command to handler via MediatR for processing
    /// 3. Handler searches for user by email address
    /// 4. If user not found, handler logs attempt and returns success (prevents email enumeration)
    /// 5. If user found, handler generates unique reset token (GUID) with 1-hour expiration
    /// 6. Handler creates VerificationToken entity with TokenType=PasswordReset
    /// 7. Handler saves token to database
    /// 8. Handler sends password reset email with token link to user's email
    /// 9. Returns success message
    /// Security: Always returns success even if email not found to prevent attackers from discovering valid emails.
    /// Used by frontend forgot password form to initiate password reset flow.
    /// </summary>
    /// <param name="command">Command containing email address for password reset</param>
    /// <returns>
    /// 200 OK with success message (always returns success for security).
    /// 500 Internal Server Error for unexpected errors during password reset request.
    /// </returns>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return Ok(new { message = "If your email is registered, you will receive a password reset link shortly." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password request");
            return StatusCode(500, new { error = "An error occurred during password reset request" });
        }
    }

    /// <summary>
    /// Resets a user's password using a valid reset token.
    /// Endpoint: POST /api/v1/auth/reset-password
    /// Logic flow:
    /// 1. Receives reset password request with reset token and new password
    /// 2. Sends command to handler via MediatR for processing
    /// 3. Handler retrieves verification token from database by token value
    /// 4. Handler validates token exists, type is PasswordReset, not expired, and not used
    /// 5. If validation fails, returns 400 Bad Request with error message
    /// 6. Handler retrieves user from database by user ID from token
    /// 7. If user not found or inactive, returns 400 Bad Request
    /// 8. Handler hashes new password using BCrypt
    /// 9. Handler updates user password hash in database
    /// 10. Handler marks verification token as used (IsUsed=true, UsedAt=current time)
    /// 11. Handler revokes all refresh tokens for user (forces re-login on all devices)
    /// 12. Returns success message
    /// Security: Token is single-use and expires in 1 hour. All sessions invalidated after reset.
    /// Used by frontend reset password form after user clicks reset link in email.
    /// </summary>
    /// <param name="command">Command containing reset token and new password</param>
    /// <returns>
    /// 200 OK with success message if password reset succeeds.
    /// 400 Bad Request if token invalid, expired, already used, or user not found.
    /// 500 Internal Server Error for unexpected errors during password reset.
    /// </returns>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return Ok(new { message = "Password reset successfully. Please login with your new password." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            return StatusCode(500, new { error = "An error occurred during password reset" });
        }
    }
    [HttpGet("reset-password")]
    public IActionResult ResetPasswordCheck() => Ok(new { status = "Healthy" });

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "Healthy", service = "Auth API", timestamp = DateTime.UtcNow });
    }
}
