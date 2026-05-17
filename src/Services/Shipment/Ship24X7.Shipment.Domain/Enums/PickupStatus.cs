namespace Ship24X7.Shipment.Domain.Enums;

/// <summary>
/// PickupStatus implementation. Provides functionality for the application.
/// </summary>
public enum PickupStatus
{
    Scheduled,
    EnRoute,
    Completed,
    Failed,
    Cancelled
}
