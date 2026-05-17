namespace Ship24X7.Notification.Application.DTOs;

/// <summary>
/// Data transfer object for NotificationPreference data. Used for API responses and data serialization.
/// </summary>
public class NotificationPreferenceResponse
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets the userId.
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// Gets or sets the emailEnabled.
    /// </summary>
    public bool EmailEnabled { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether sms is enabled.
    /// </summary>
    public bool SmsEnabled { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether push is enabled.
    /// </summary>
    public bool PushEnabled { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether inapp is enabled.
    /// </summary>
    public bool InAppEnabled { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether bookingconfirmation is enabled.
    /// </summary>
    public bool BookingConfirmationEnabled { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether deliveryconfirmation is enabled.
    /// </summary>
    public bool DeliveryConfirmationEnabled { get; set; }
    /// <summary>
    /// Gets or sets the paymentConfirmationEnabled.
    /// </summary>
    public bool PaymentConfirmationEnabled { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether delayalerts is enabled.
    /// </summary>
    public bool DelayAlertsEnabled { get; set; }
}
