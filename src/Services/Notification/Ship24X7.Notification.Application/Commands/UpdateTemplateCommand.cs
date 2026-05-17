using MediatR;
using Ship24X7.Notification.Domain.ValueObjects;

namespace Ship24X7.Notification.Application.Commands;

/// <summary>
/// Represents a command to update an existing notification template's content, metadata, or active status.
/// Encapsulates all updatable template fields including name, subject, body, placeholders, and activation state.
/// Used by administrators to modify templates without creating new versions, maintaining template ID references in notification logs.
/// Implements CQRS pattern with Unit return type indicating successful update without returning data.
/// </summary>
public class UpdateTemplateCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets or sets the unique identifier of the template to update.
    /// Injected by controller from route parameter. Handler uses this to locate existing template in database.
    /// If template not found, handler throws InvalidOperationException resulting in 404 Not Found response.
    /// </summary>
    public Guid TemplateId { get; set; }
    
    /// <summary>
    /// Gets or sets the updated name of the template.
    /// Must remain unique across all templates. Used for identification in admin UI and template selection.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the updated template type indicating business purpose.
    /// Values: Welcome, Verification, PasswordReset, OrderConfirmation, ShipmentUpdate, DeliveryNotification, PaymentReceipt, etc.
    /// Changing type may affect how template is categorized and filtered in admin UI.
    /// </summary>
    public TemplateType Type { get; set; }
    
    /// <summary>
    /// Gets or sets the updated notification delivery channel.
    /// Values: Email, SMS, Push. Changing channel requires updating body template to match new channel's format constraints.
    /// Handler validates template content matches channel requirements after update.
    /// </summary>
    public NotificationChannel Channel { get; set; }
    
    /// <summary>
    /// Gets or sets the updated subject line for Email channel templates.
    /// Required for Email channel, ignored for SMS and Push. Supports placeholders for dynamic subjects.
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the updated template body content with Razor syntax placeholders.
    /// Must contain all placeholders listed in RequiredPlaceholders array. Controller validates this before sending command.
    /// Handler uses RazorTemplateRenderer to validate template syntax before saving.
    /// </summary>
    public string BodyTemplate { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the updated array of required placeholder names.
    /// Controller validates all placeholders exist in BodyTemplate before sending command.
    /// Changing required placeholders may break existing notification sending code that uses this template.
    /// </summary>
    public string[] RequiredPlaceholders { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// Gets or sets whether the template is active and available for use.
    /// Inactive templates cannot be selected for new notifications but existing notification logs retain reference.
    /// Used to deprecate old templates without deleting them, preserving audit trail.
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Gets or sets the unique identifier of the administrator updating this template.
    /// Injected by controller from JWT claims. Used for audit trail tracking who modified template and when.
    /// Stored in UpdatedBy and UpdatedAt fields of NotificationTemplate entity.
    /// </summary>
    public Guid UpdatedBy { get; set; }
}
