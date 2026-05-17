using Ship24X7.Shared.Domain;
using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Domain.Events;

/// <summary>
/// Domain event raised when PickupScheduled occurs. Used for event-driven architecture and integration.
/// </summary>
public class PickupScheduled : BaseDomainEvent
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
    /// Gets or sets the confirmationationNumber.
    /// </summary>
    public string ConfirmationNumber { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the pickupdate.
    /// </summary>
    public DateTime PickupDate { get; set; }
    /// <summary>
    /// Gets or sets the timeslot.
    /// </summary>
    public PickupTimeSlot TimeSlot { get; set; }
}
