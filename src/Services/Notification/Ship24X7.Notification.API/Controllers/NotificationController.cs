using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ship24X7.Notification.Application.Commands;
using Ship24X7.Notification.Application.Queries;
using System.Security.Claims;

namespace Ship24X7.Notification.API.Controllers;

/// <summary>
/// Handles HTTP requests for notification management operations in the Ship24X7 platform.
/// Provides endpoints for retrieving notification history and managing user notification preferences.
/// Requires JWT authentication for all operations. Extracts user identity from JWT claims to ensure users can only access their own notifications.
/// Uses CQRS pattern with MediatR to dispatch commands and queries to application layer handlers.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<NotificationController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationController"/> class.
    /// Sets up mediator for CQRS command/query dispatching and logger for error tracking.
    /// </summary>
    /// <param name="mediator">MediatR instance for sending commands and queries to application layer handlers.</param>
    /// <param name="logger">Logger instance for recording errors and diagnostic information.</param>
    public NotificationController(IMediator mediator, ILogger<NotificationController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves paginated notification history for the authenticated user.
    /// Logic flow: 1) Extracts user ID from JWT claims, 2) Validates authentication, 3) Creates query with pagination parameters,
    /// 4) Dispatches query to handler via MediatR, 5) Returns notification logs ordered by most recent first.
    /// Supports pagination to handle large notification histories efficiently.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve (1-based index). Defaults to 1 for first page.</param>
    /// <param name="pageSize">Number of notifications per page. Defaults to 50. Maximum recommended is 100.</param>
    /// <returns>
    /// 200 OK with array of notification log DTOs containing channel, status, timestamp, and message details.
    /// 401 Unauthorized if user is not authenticated or JWT token is invalid.
    /// 500 Internal Server Error if database query fails or unexpected error occurs.
    /// </returns>
    [HttpGet("history")]
    public async Task<IActionResult> GetNotificationHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            var query = new GetNotificationLogQuery
            {
                UserId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var notifications = await _mediator.Send(query);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification history");
            return StatusCode(500, new { error = "An error occurred while retrieving notification history" });
        }
    }

    /// <summary>
    /// Retrieves notification preferences for the authenticated user.
    /// Logic flow: 1) Extracts user ID from JWT claims, 2) Validates authentication, 3) Queries database for user preferences,
    /// 4) Returns preference settings including enabled channels (Email/SMS/Push), event subscriptions, and quiet hours.
    /// If preferences don't exist, returns 404 indicating user needs to set up preferences first.
    /// </summary>
    /// <returns>
    /// 200 OK with notification preference DTO containing channel enablement flags, subscribed event types, and quiet hours configuration.
    /// 401 Unauthorized if user is not authenticated or JWT token is invalid.
    /// 404 Not Found if user has no preference record (first-time user or preferences deleted).
    /// 500 Internal Server Error if database query fails or unexpected error occurs.
    /// </returns>
    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences()
    {
        try
        {
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            var query = new GetUserPreferenceQuery { UserId = userId };
            var preferences = await _mediator.Send(query);

            if (preferences == null)
            {
                return NotFound(new { error = "Preferences not found" });
            }

            return Ok(preferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification preferences");
            return StatusCode(500, new { error = "An error occurred while retrieving preferences" });
        }
    }

    /// <summary>
    /// Updates notification preferences for the authenticated user.
    /// Logic flow: 1) Extracts user ID from JWT claims, 2) Validates authentication, 3) Injects user ID into command,
    /// 4) Validates command using FluentValidation rules, 5) Updates or creates preference record in database,
    /// 6) Returns success confirmation. Allows users to enable/disable channels, subscribe/unsubscribe from events, and set quiet hours.
    /// </summary>
    /// <param name="command">Update preference command containing channel settings (EmailEnabled, SmsEnabled, PushEnabled), event subscriptions, and quiet hours configuration.</param>
    /// <returns>
    /// 200 OK with success message if preferences updated successfully.
    /// 400 Bad Request if validation fails (invalid quiet hours, conflicting settings).
    /// 401 Unauthorized if user is not authenticated or JWT token is invalid.
    /// 500 Internal Server Error if database update fails or unexpected error occurs.
    /// </returns>
    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferenceCommand command)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            command.UserId = userId;
            await _mediator.Send(command);

            return Ok(new { message = "Preferences updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification preferences");
            return StatusCode(500, new { error = "An error occurred while updating preferences" });
        }
    }

    /// <summary>
    /// Extracts the user ID from JWT token claims.
    /// Logic: Searches for NameIdentifier claim in JWT token, attempts to parse as GUID, returns Guid.Empty if not found or invalid.
    /// Used internally to identify the authenticated user for all operations.
    /// </summary>
    /// <returns>User's unique identifier (GUID) if found and valid; otherwise Guid.Empty indicating authentication failure.</returns>
    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
