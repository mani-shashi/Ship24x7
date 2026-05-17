namespace Ship24X7.Notification.Domain.ValueObjects;

/// <summary>
/// Value object representing TemplateType in the domain model. Immutable type with value-based equality.
/// </summary>
public enum TemplateType
{
    BookingConfirmation,
    DeliveryConfirmation,
    PaymentConfirmation,
    ShipmentDelayed,
    ShipmentOutForDelivery,
    PasswordReset,
    EmailVerification,
    MfaEnabled,
    PickupScheduled
}
