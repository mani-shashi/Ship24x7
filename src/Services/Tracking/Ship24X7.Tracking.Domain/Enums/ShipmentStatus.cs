namespace Ship24X7.Tracking.Domain.Enums;

/// <summary>
/// ShipmentStatus implementation. Provides functionality for the application.
/// </summary>
public enum ShipmentStatus
{
    Draft,
    Booked,
    PaymentPending,
    Paid,
    PickedUp,
    InTransit,
    OutForDelivery,
    Delivered,
    Failed,
    Returned,
    Cancelled,
    Delayed,
    PaymentFailed
}
