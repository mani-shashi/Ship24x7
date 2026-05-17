using Ship24X7.Shared.Domain;

namespace Ship24X7.Notification.Domain.Entities;

/// <summary>
/// Represents a user's notification preferences for channels and event subscriptions in the Ship24X7 platform.
/// Stores user consent and opt-out decisions for different notification channels (Email/SMS/Push/InApp) and event types.
/// Used by SendNotificationCommandHandler to respect user preferences and skip notifications for disabled channels or events.
/// Implements user privacy and consent management by allowing granular control over notification delivery.
/// One preference record per user. Defaults to all channels and events enabled for new users.
/// Inherits audit fields (CreatedAt, UpdatedAt) from BaseEntity for tracking when preferences were last modified.
/// </summary>
public class NotificationPreference : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this preference record.
    /// Primary key used for preference queries and updates.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the unique identifier of the user who owns these preferences.
    /// Foreign key to User entity in Auth service. One-to-one relationship: one user has one preference record.
    /// Used to query preferences by user ID when checking if notification should be sent.
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Gets or sets whether email notifications are enabled for this user.
    /// When false, SendNotificationCommandHandler skips email delivery even if notification specifies Email channel.
    /// Allows users to opt out of all email notifications while keeping other channels active.
    /// Defaults to true for new users (opt-out model for better user engagement).
    /// </summary>
    public bool EmailEnabled { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether SMS notifications are enabled for this user.
    /// When false, SendNotificationCommandHandler skips SMS delivery even if notification specifies SMS channel.
    /// Allows users to opt out of SMS notifications to avoid message charges or reduce interruptions.
    /// Defaults to true for new users but users often disable due to SMS costs.
    /// </summary>
    public bool SmsEnabled { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether push notifications are enabled for this user.
    /// When false, SendNotificationCommandHandler skips push delivery even if notification specifies Push channel.
    /// Allows users to disable mobile/browser push notifications while keeping email/SMS active.
    /// Defaults to true for new users. Users may disable if they find push notifications too intrusive.
    /// </summary>
    public bool PushEnabled { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether in-app notifications are enabled for this user.
    /// When false, in-app notification bell/inbox won't show new notifications in the web/mobile app UI.
    /// Allows users to disable in-app alerts while keeping external channels (email/SMS) active.
    /// Defaults to true for new users. Rarely disabled as in-app notifications are non-intrusive.
    /// </summary>
    public bool InAppEnabled { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether the user wants to receive booking/shipment creation confirmation notifications.
    /// When false, notifications triggered by ShipmentBooked event are suppressed for this user regardless of channel.
    /// Allows users to opt out of initial booking confirmations if they find them redundant.
    /// Defaults to true. Some users disable if they track bookings through other means.
    /// </summary>
    public bool BookingConfirmationEnabled { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether the user wants to receive delivery completion notifications.
    /// When false, notifications triggered by ShipmentDelivered event are suppressed for this user regardless of channel.
    /// Most users keep this enabled as delivery confirmation is critical information requiring immediate attention.
    /// Defaults to true. Rarely disabled as users want to know when their shipment arrives.
    /// </summary>
    public bool DeliveryConfirmationEnabled { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether the user wants to receive payment confirmation notifications.
    /// When false, notifications triggered by PaymentCaptured event are suppressed for this user regardless of channel.
    /// Allows users to opt out of payment receipts if they track payments through bank statements or other means.
    /// Defaults to true. Some users disable if they receive payment confirmations from payment gateway directly.
    /// </summary>
    public bool PaymentConfirmationEnabled { get; set; } = true;
    
    /// <summary>
    /// Gets or sets whether the user wants to receive shipment delay alert notifications.
    /// When false, notifications triggered by ShipmentDelayed event are suppressed for this user regardless of channel.
    /// Most users keep this enabled as delay alerts require immediate attention and may need action (reschedule delivery, contact support).
    /// Defaults to true. Rarely disabled as delays are time-sensitive and important to users.
    /// </summary>
    public bool DelayAlertsEnabled { get; set; } = true;
}
