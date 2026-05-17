using Ship24X7.Shared.Domain;
using Ship24X7.Tracking.Domain.Enums;

namespace Ship24X7.Tracking.Domain.Events;

/// <summary>
/// Domain event raised when ShipmentDelayed occurs. Used for event-driven architecture and integration.
/// </summary>
public class ShipmentDelayed : BaseDomainEvent
{
    /// <summary>
    /// Gets or sets the shipmentId.
    /// </summary>
    public Guid ShipmentId { get; set; }
    /// <summary>
    /// Gets or sets the trackingNumber.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Current status. Null when the event has an unrecognised status string.
    /// </summary>
    public ShipmentStatus? CurrentStatus { get; set; }
    /// <summary>
    /// Gets or sets the exceptionreason.
    /// </summary>
    public string ExceptionReason { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the location.
    /// </summary>
    public string Location { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the delayedat.
    /// </summary>
    public DateTime DelayedAt { get; set; }
}
