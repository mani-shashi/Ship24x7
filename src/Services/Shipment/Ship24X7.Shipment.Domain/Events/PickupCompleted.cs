using Ship24X7.Shared.Domain;

namespace Ship24X7.Shipment.Domain.Events;

/// <summary>
/// Domain event raised when PickupCompleted occurs. Used for event-driven architecture and integration.
/// </summary>
public class PickupCompleted : BaseDomainEvent
{
    /// <summary>
    /// Gets or sets the pickupid.
    /// </summary>
    public Guid PickupId { get; set; }
    /// <summary>
    /// Gets or sets the shipmentId.
    /// </summary>
    public Guid ShipmentId { get; set; }
    /// <summary>
    /// Gets or sets the trackingNumber.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the completedat.
    /// </summary>
    public DateTime CompletedAt { get; set; }
    /// <summary>
    /// Gets or sets the driverid.
    /// </summary>
    public Guid? DriverId { get; set; }
}
