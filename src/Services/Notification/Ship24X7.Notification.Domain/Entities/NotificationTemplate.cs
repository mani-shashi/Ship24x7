using Ship24X7.Shared.Domain;
using Ship24X7.Notification.Domain.ValueObjects;

namespace Ship24X7.Notification.Domain.Entities;

/// <summary>
/// Represents a reusable notification template for email, SMS, or push notifications in the Ship24X7 platform.
/// Defines the structure and content of notifications with Razor syntax placeholders for dynamic data injection.
/// Templates are created by administrators and used by SendNotificationCommand to generate personalized notifications.
/// Supports multiple channels (Email/SMS/Push) and template types (Welcome/Verification/OrderConfirmation/etc).
/// Includes validation rules for required placeholders to ensure all necessary data is provided at send time.
/// Inherits audit fields (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) from BaseEntity for change tracking.
/// </summary>
public class NotificationTemplate : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this notification template.
    /// Primary key used for template selection in SendNotificationCommand and template management operations.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the unique name of the template for identification and selection.
    /// Must be unique across all templates. Used by administrators to identify templates in management UI.
    /// Examples: "Welcome Email", "Order Confirmation SMS", "Delivery Alert Push", "Password Reset Email".
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the template type indicating the business purpose and scenario.
    /// Values: Welcome, Verification, PasswordReset, OrderConfirmation, ShipmentUpdate, DeliveryNotification, PaymentReceipt, DelayAlert, etc.
    /// Used for categorizing templates in admin UI and filtering templates by business scenario.
    /// Helps administrators organize templates and developers select appropriate template for each event.
    /// </summary>
    public TemplateType Type { get; set; }
    
    /// <summary>
    /// Gets or sets the notification delivery channel this template is designed for.
    /// Values: Email, SMS, Push. Determines template format constraints and rendering behavior.
    /// Email templates support HTML tags and longer content. SMS templates must be plain text under 160 chars.
    /// Push templates should be short text (max 200 chars recommended) for mobile notification display.
    /// </summary>
    public NotificationChannel Channel { get; set; }
    
    /// <summary>
    /// Gets or sets the subject line template for Email channel notifications.
    /// Required for Email channel, ignored for SMS and Push channels. Supports {{placeholder}} syntax for dynamic subjects.
    /// Rendered by RazorTemplateRenderer with placeholder data before sending.
    /// Example: "Your order {{OrderNumber}} has been shipped and will arrive on {{DeliveryDate}}".
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the template body content with Razor syntax placeholders.
    /// Contains text and {{PlaceholderName}} markers that are replaced with actual values during rendering.
    /// For Email: supports HTML tags for formatting (bold, links, tables, etc.).
    /// For SMS: plain text only, should be concise (under 160 chars to avoid multi-part messages).
    /// For Push: short text (max 200 chars recommended) for mobile notification display.
    /// Example: "Hello {{CustomerName}}, your shipment {{TrackingNumber}} will arrive on {{DeliveryDate}}. Track it here: {{TrackingUrl}}".
    /// </summary>
    public string BodyTemplate { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the array of placeholder names that must be provided when using this template.
    /// Validated by TemplateController before saving to ensure all placeholders exist in BodyTemplate.
    /// SendNotificationCommand must provide values for all these placeholders in PlaceholderData dictionary.
    /// Missing placeholders cause rendering errors, so this validation prevents runtime failures.
    /// Example: ["CustomerName", "TrackingNumber", "DeliveryDate", "TrackingUrl"].
    /// </summary>
    public string[] RequiredPlaceholders { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// Gets or sets whether the template is active and available for use in notifications.
    /// Inactive templates cannot be selected for new notifications but existing notification logs retain reference.
    /// Used to deprecate old templates without deleting them, preserving audit trail and notification history.
    /// Administrators can deactivate templates to prevent use while keeping them for reference.
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the navigation property to notification logs that used this template.
    /// One-to-many relationship: one template can be used by many notification logs.
    /// Allows querying which notifications were sent using this template for analytics and debugging.
    /// Lazy-loaded collection, not populated unless explicitly included in query.
    /// </summary>
    public ICollection<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
}
