using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.Queries;
using System.Security.Claims;

namespace Ship24X7.Auth.API.Controllers;

/// <summary>
/// Handles user profile management: profile updates, password changes,
/// address book CRUD, and notification/display preferences.
/// All endpoints require a valid JWT Bearer token.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(IMediator mediator, ILogger<ProfileController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }

    // ─── Profile ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Updates the authenticated user's display name, phone number, and optional profile photo URL.
    /// Endpoint: PUT /api/v1/auth/me
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { error = "Invalid token" });

        try
        {
            var command = new UpdateProfileCommand
            {
                UserId = userId.Value,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                ProfilePhotoUrl = request.ProfilePhotoUrl
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while updating profile" });
        }
    }

    /// <summary>
    /// Changes the authenticated user's password.
    /// Endpoint: PUT /api/v1/auth/me/password
    /// </summary>
    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { error = "Invalid token" });

        try
        {
            var command = new ChangePasswordCommand
            {
                UserId = userId.Value,
                CurrentPassword = request.CurrentPassword,
                NewPassword = request.NewPassword
            };

            await _mediator.Send(command);
            return Ok(new { message = "Password changed successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while changing password" });
        }
    }

    // ─── Address Book ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all saved addresses for the authenticated user.
    /// Endpoint: GET /api/v1/auth/addresses
    /// </summary>
    [HttpGet("addresses")]
    public async Task<IActionResult> GetAddresses()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { error = "Invalid token" });

        try
        {
            var result = await _mediator.Send(new GetAddressesQuery { UserId = userId.Value });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching addresses for user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while fetching addresses" });
        }
    }

    /// <summary>
    /// Creates a new saved address.
    /// Endpoint: POST /api/v1/auth/addresses
    /// </summary>
    [HttpPost("addresses")]
    public async Task<IActionResult> CreateAddress([FromBody] SaveAddressRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { error = "Invalid token" });

        try
        {
            var command = MapToSaveCommand(userId.Value, null, request);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAddresses), result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating address for user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while saving address" });
        }
    }

    /// <summary>
    /// Updates an existing saved address.
    /// Endpoint: PUT /api/v1/auth/addresses/{id}
    /// </summary>
    [HttpPut("addresses/{id:guid}")]
    public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] SaveAddressRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { error = "Invalid token" });

        try
        {
            var command = MapToSaveCommand(userId.Value, id, request);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating address {AddressId} for user {UserId}", id, userId);
            return StatusCode(500, new { error = "An error occurred while updating address" });
        }
    }

    /// <summary>
    /// Deletes a saved address.
    /// Endpoint: DELETE /api/v1/auth/addresses/{id}
    /// </summary>
    [HttpDelete("addresses/{id:guid}")]
    public async Task<IActionResult> DeleteAddress(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { error = "Invalid token" });

        try
        {
            await _mediator.Send(new DeleteAddressCommand { UserId = userId.Value, AddressId = id });
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting address {AddressId} for user {UserId}", id, userId);
            return StatusCode(500, new { error = "An error occurred while deleting address" });
        }
    }

    // ─── Preferences ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the authenticated user's notification and display preferences.
    /// Returns defaults if the user has not saved preferences yet.
    /// Endpoint: GET /api/v1/auth/preferences
    /// </summary>
    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { error = "Invalid token" });

        try
        {
            var result = await _mediator.Send(new GetPreferencesQuery { UserId = userId.Value });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching preferences for user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while fetching preferences" });
        }
    }

    /// <summary>
    /// Creates or updates the authenticated user's preferences (upsert).
    /// Endpoint: PUT /api/v1/auth/preferences
    /// </summary>
    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new { error = "Invalid token" });

        try
        {
            var command = new UpdatePreferencesCommand
            {
                UserId = userId.Value,
                EmailNotifications = request.EmailNotifications,
                SmsNotifications = request.SmsNotifications,
                PushNotifications = request.PushNotifications,
                MarketingEmails = request.MarketingEmails,
                Theme = request.Theme,
                Language = request.Language
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating preferences for user {UserId}", userId);
            return StatusCode(500, new { error = "An error occurred while updating preferences" });
        }
    }

    // ─── Private helpers ─────────────────────────────────────────────────────

    private static SaveAddressCommand MapToSaveCommand(Guid userId, Guid? addressId, SaveAddressRequest r) =>
        new()
        {
            UserId = userId,
            AddressId = addressId,
            Label = r.Label,
            ContactName = r.ContactName,
            ContactPhone = r.ContactPhone,
            AddressLine1 = r.AddressLine1,
            AddressLine2 = r.AddressLine2,
            City = r.City,
            State = r.State,
            PostalCode = r.PostalCode,
            Country = r.Country,
            Type = r.Type,
            IsDefault = r.IsDefault
        };
}

// ─── Request models (inline — keeps the controller file self-contained) ──────

/// <summary>Request body for PUT /api/v1/auth/me</summary>
public record UpdateProfileRequest(
    string FullName,
    string PhoneNumber,
    string? ProfilePhotoUrl);

/// <summary>Request body for PUT /api/v1/auth/me/password</summary>
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);

/// <summary>Request body for POST/PUT /api/v1/auth/addresses</summary>
public record SaveAddressRequest(
    string Label,
    string ContactName,
    string ContactPhone,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string PostalCode,
    string Country,
    string Type,
    bool IsDefault);

/// <summary>Request body for PUT /api/v1/auth/preferences</summary>
public record UpdatePreferencesRequest(
    bool EmailNotifications,
    bool SmsNotifications,
    bool PushNotifications,
    bool MarketingEmails,
    string Theme,
    string Language);
