using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ship24X7.Notification.Application.Commands;
using Ship24X7.Notification.Application.Queries;
using System.Security.Claims;
namespace Ship24X7.Notification.API.Controllers;

/// <summary>
/// Handles HTTP requests for notification template management in the Ship24X7 platform.
/// Provides endpoints for creating, updating, and retrieving notification templates used for email, SMS, and push notifications.
/// Restricted to System_Admin and Admin_User roles only. Templates use Razor syntax with placeholders for dynamic content.
/// Validates that template body contains all required placeholders before saving to prevent runtime errors.
/// Uses CQRS pattern with MediatR to dispatch commands and queries to application layer handlers.
/// </summary>
[ApiController]
[Route("api/v1/Notification/templates")]
[Authorize(Roles = "System_Admin,Admin_User")]
public class TemplateController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TemplateController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateController"/> class.
    /// Sets up mediator for CQRS command/query dispatching and logger for error tracking.
    /// </summary>
    /// <param name="mediator">MediatR instance for sending commands and queries to application layer handlers.</param>
    /// <param name="logger">Logger instance for recording errors, warnings, and diagnostic information.</param>
    public TemplateController(IMediator mediator, ILogger<TemplateController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all notification templates with optional filtering by active status.
    /// Logic flow: 1) Creates query with optional IsActive filter, 2) Dispatches query to handler via MediatR,
    /// 3) Handler queries database and returns templates ordered by name, 4) Returns template list with metadata.
    /// Used by admins to view and manage available templates for different notification types and channels.
    /// </summary>
    /// <param name="isActive">Optional filter: true returns only active templates, false returns only inactive, null returns all templates.</param>
    /// <returns>
    /// 200 OK with array of template DTOs containing ID, name, type, channel, subject, body template, required placeholders, and active status.
    /// 500 Internal Server Error if database query fails or unexpected error occurs.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetTemplates([FromQuery] bool? isActive = null)
    {
        try
        {
            var query = new GetTemplateListQuery { IsActive = isActive };
            var templates = await _mediator.Send(query);
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving templates");
            return StatusCode(500, new { error = "An error occurred while retrieving templates" });
        }
    }

    /// <summary>
    /// Creates a new notification template for email, SMS, or push notifications.
    /// Logic flow: 1) Extracts admin user ID from JWT claims, 2) Validates authentication, 3) Validates template placeholders exist in body,
    /// 4) Injects creator ID into command, 5) Validates command using FluentValidation rules, 6) Creates template record in database,
    /// 7) Returns created template ID. Placeholder validation ensures template can be rendered without errors at runtime.
    /// </summary>
    /// <param name="command">Create template command containing name, type (Welcome/Verification/PasswordReset/etc), channel (Email/SMS/Push), subject, body template with {{placeholders}}, and required placeholder names array.</param>
    /// <returns>
    /// 201 Created with template ID and location header pointing to GetTemplates endpoint.
    /// 400 Bad Request if validation fails (missing placeholders in body, invalid template syntax, duplicate name).
    /// 401 Unauthorized if user is not authenticated or JWT token is invalid.
    /// 403 Forbidden if user doesn't have System_Admin or Admin_User role.
    /// 500 Internal Server Error if database insert fails or unexpected error occurs.
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateCommand command)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            command.CreatedBy = userId;

            // Validate template placeholders
            if (!ValidateTemplatePlaceholders(command.BodyTemplate, command.RequiredPlaceholders))
            {
                return BadRequest(new { error = "Template body does not contain all required placeholders" });
            }

            var templateId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetTemplates), new { id = templateId }, new { templateId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template");
            return StatusCode(500, new { error = "An error occurred while creating template" });
        }
    }

    /// <summary>
    /// Updates an existing notification template.
    /// Logic flow: 1) Extracts admin user ID from JWT claims, 2) Validates authentication, 3) Injects template ID and updater ID into command,
    /// 4) Validates template placeholders exist in body, 5) Validates command using FluentValidation rules,
    /// 6) Retrieves existing template from database, 7) Updates template fields, 8) Saves changes to database.
    /// Placeholder validation ensures updated template can be rendered without errors at runtime.
    /// </summary>
    /// <param name="id">The unique identifier of the template to update.</param>
    /// <param name="command">Update template command containing updated name, subject, body template with {{placeholders}}, required placeholder names array, and active status.</param>
    /// <returns>
    /// 200 OK with success message if template updated successfully.
    /// 400 Bad Request if validation fails (missing placeholders in body, invalid template syntax).
    /// 401 Unauthorized if user is not authenticated or JWT token is invalid.
    /// 403 Forbidden if user doesn't have System_Admin or Admin_User role.
    /// 404 Not Found if template with specified ID doesn't exist.
    /// 500 Internal Server Error if database update fails or unexpected error occurs.
    /// </returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] UpdateTemplateCommand command)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty)
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            command.TemplateId = id;
            command.UpdatedBy = userId;

            // Validate template placeholders
            if (!ValidateTemplatePlaceholders(command.BodyTemplate, command.RequiredPlaceholders))
            {
                return BadRequest(new { error = "Template body does not contain all required placeholders" });
            }

            await _mediator.Send(command);
            return Ok(new { message = "Template updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template");
            return StatusCode(500, new { error = "An error occurred while updating template" });
        }
    }

    /// <summary>
    /// Validates that all required placeholders are present in the template body.
    /// Logic: Iterates through required placeholder names, wraps each in double curly braces {{placeholder}},
    /// checks if template body contains the formatted placeholder string. Logs warning for missing placeholders.
    /// Prevents runtime errors when rendering templates by ensuring all required data fields have corresponding placeholders.
    /// </summary>
    /// <param name="template">The template body string containing text and {{placeholder}} markers.</param>
    /// <param name="requiredPlaceholders">Array of placeholder names that must exist in the template (without curly braces).</param>
    /// <returns>True if all required placeholders are found in template body; false if any placeholder is missing.</returns>
    private bool ValidateTemplatePlaceholders(string template, string[] requiredPlaceholders)
    {
        foreach (var placeholder in requiredPlaceholders)
        {
            var placeholderPattern = $"{{{{{placeholder}}}}}";
            if (!template.Contains(placeholderPattern))
            {
                _logger.LogWarning("Template missing required placeholder: {Placeholder}", placeholder);
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Extracts the user ID from JWT token claims.
    /// Logic: Searches for NameIdentifier claim in JWT token, attempts to parse as GUID, returns Guid.Empty if not found or invalid.
    /// Used internally to identify the authenticated admin user for audit tracking (CreatedBy/UpdatedBy fields).
    /// </summary>
    /// <returns>Admin user's unique identifier (GUID) if found and valid; otherwise Guid.Empty indicating authentication failure.</returns>
    private Guid GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Deletes a notification template by its unique identifier.
    /// Endpoint: DELETE /api/v1/Notification/templates/{id}
    /// </summary>
    /// <param name="id">The unique identifier of the template to delete.</param>
    /// <returns>
    /// 200 OK with success message if template deleted successfully.
    /// 404 Not Found if template with specified ID doesn't exist.
    /// 500 Internal Server Error if database delete fails or unexpected error occurs.
    /// </returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "System_Admin,Admin_User")]
    public async Task<IActionResult> DeleteTemplate(Guid id)
    {
        try
        {
            var command = new DeleteTemplateCommand { TemplateId = id };
            await _mediator.Send(command);
            return Ok(new { message = "Notification template deleted successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete notification template {TemplateId}", id);
            return StatusCode(500, new { error = "An internal error occurred" });
        }
    }
}
