using MediatR;

namespace Ship24X7.Notification.Application.Commands;

/// <summary>
/// Represents a command to update a user's notification preferences for channels and event subscriptions.
/// Encapsulates all preference settings including channel enablement (Email/SMS/Push/InApp) and event type subscriptions.
/// Used by users to control which notifications they receive and through which channels, implementing user consent and preference management.
/// Implements CQRS pattern with Unit return type indicating successful update without returning data.
/// Handler creates preference record if it doesn't exist (first-time setup) or updates existing record.
/// </summary>
public class UpdatePreferenceCommand : IRequest<Unit>
{
    /// <summary>
    /// Gets or sets the unique identifier of the user whose preferences are being updated.
    /// Injected by controller from JWT claims. Ensures users can only update their own preferences.
    /// Used as primary key to locate or create NotificationPreference record in database.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets whether email notifications are enabled for this user.
    /// When false, SendNotificationCommandHandler skips email delivery even if notification specifies Email channel.
    /// Allows users to opt out of email notifications while keeping other channels active.
    /// </summary>
    public bool EmailEnabled { get; set; }
    
    /// <summary>
    /// Gets or sets whether SMS notifications are enabled for this user.
    /// When false, SendNotificationCommandHandler skips SMS delivery even if notification specifies SMS channel.
    /// Allows users to opt out of SMS notifications to avoid message charges or reduce interruptions.
    /// </summary>
    public bool SmsEnabled { get; set; }
    
    /// <summary>
    /// Gets or sets whether push notifications are enabled for this user.
    /// When false, SendNotificationCommandHandler skips push delivery even if notification specifies Push channel.
    /// Allows users to disable mobile/browser push notifications while keeping email/SMS active.
    /// </summary>
    public bool PushEnabled { get; set; }
    
    /// <summary>
    /// Gets or sets whether in-app notifications are enabled for this user.
    /// When false, in-app notification bell/inbox won't show new notifications.
    /// Allows users to disable in-app alerts while keeping external channels (email/SMS) active.
    /// </summary>
    public bool InAppEnabled { get; set; }
    
    /// <summary>
    /// Gets or sets whether the user wants to receive booking/shipment creation confirmation notifications.
    /// When false, notifications triggered by ShipmentBooked event are suppressed for this user.
    /// Allows users to opt out of initial booking confirmations if they find them redundant.
    /// </summary>
    public bool BookingConfirmationEnabled { get; set; }
    
    /// <summary>
    /// Gets or sets whether the user wants to receive delivery completion notifications.
    /// When false, notifications triggered by ShipmentDelivered event are suppressed for this user.
    /// Most users keep this enabled as delivery confirmation is critical information.
    /// </summary>
    public bool DeliveryConfirmationEnabled { get; set; }
    
    /// <summary>
    /// Gets or sets whether the user wants to receive payment confirmation notifications.
    /// When false, notifications triggered by PaymentCaptured event are suppressed for this user.
    /// Allows users to opt out of payment receipts if they track payments through other means.
    /// </summary>
    public bool PaymentConfirmationEnabled { get; set; }
    
    /// <summary>
    /// Gets or sets whether the user wants to receive shipment delay alert notifications.
    /// When false, notifications triggered by ShipmentDelayed event are suppressed for this user.
    /// Most users keep this enabled as delay alerts require immediate attention and action.
    /// </summary>
    public bool DelayAlertsEnabled { get; set; }
}
