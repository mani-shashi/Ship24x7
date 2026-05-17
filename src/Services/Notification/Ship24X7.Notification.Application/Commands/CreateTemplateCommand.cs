using MediatR;
using Ship24X7.Notification.Domain.ValueObjects;

namespace Ship24X7.Notification.Application.Commands;

/// <summary>
/// Represents a command to create a new notification template for email, SMS, or push notifications.
/// Encapsulates template metadata, content with Razor placeholders, and required placeholder validation rules.
/// Used by administrators to define reusable notification templates for different business events and channels.
/// Implements CQRS pattern by returning the created template ID for immediate use in notification sending.
/// </summary>
public class CreateTemplateCommand : IRequest<Guid>
{
    /// <summary>
    /// Gets or sets the unique name of the template for identification and selection.
    /// Must be unique across all templates. Used by admins to identify templates in management UI.
    /// Examples: "Welcome Email", "Order Confirmation SMS", "Delivery Alert Push".
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the template type indicating the business purpose.
    /// Values: Welcome, Verification, PasswordReset, OrderConfirmation, ShipmentUpdate, DeliveryNotification, PaymentReceipt, etc.
    /// Used for categorization and filtering templates by business scenario.
    /// </summary>
    public TemplateType Type { get; set; }
    
    /// <summary>
    /// Gets or sets the notification delivery channel this template is designed for.
    /// Values: Email, SMS, Push. Determines template format constraints (HTML for Email, plain text for SMS, short text for Push).
    /// Handler validates template content matches channel requirements (e.g., SMS templates must be under 160 characters).
    /// </summary>
    public NotificationChannel Channel { get; set; }
    
    /// <summary>
    /// Gets or sets the subject line for Email channel templates.
    /// Required for Email channel, ignored for SMS and Push. Supports placeholders for dynamic subjects.
    /// Example: "Your order {{OrderNumber}} has been shipped".
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the template body content with Razor syntax placeholders.
    /// Contains text and {{PlaceholderName}} markers that will be replaced with actual values during rendering.
    /// For Email: supports HTML tags. For SMS: plain text only. For Push: short text (max 200 chars recommended).
    /// Example: "Hello {{CustomerName}}, your shipment {{TrackingNumber}} will arrive on {{DeliveryDate}}."
    /// </summary>
    public string BodyTemplate { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the array of placeholder names that must be provided when using this template.
    /// Handler validates that all required placeholders exist in BodyTemplate before saving.
    /// SendNotificationCommand must provide values for all these placeholders in PlaceholderData dictionary.
    /// Example: ["CustomerName", "TrackingNumber", "DeliveryDate"].
    /// </summary>
    public string[] RequiredPlaceholders { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// Gets or sets the unique identifier of the administrator creating this template.
    /// Injected by controller from JWT claims. Used for audit trail and tracking template authorship.
    /// Stored in CreatedBy field of NotificationTemplate entity.
    /// </summary>
    public Guid CreatedBy { get; set; }
}
