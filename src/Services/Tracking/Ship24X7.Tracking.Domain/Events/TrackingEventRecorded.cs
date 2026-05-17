using Ship24X7.Shared.Domain;
using Ship24X7.Tracking.Domain.Enums;

namespace Ship24X7.Tracking.Domain.Events;

/// <summary>
/// Domain event raised when TrackingRecorded occurs. Used for event-driven architecture and integration.
/// </summary>
public class TrackingEventRecorded : BaseDomainEvent
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
    /// Gets or sets the status. Null when the event was recorded with an unrecognised status string.
    /// </summary>
    public ShipmentStatus? Status { get; set; }
    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the location.
    /// </summary>
    public string Location { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the eventtimestamp.
    /// </summary>
    public DateTime EventTimestamp { get; set; }
    /// <summary>
    /// Gets or sets the isexception.
    /// </summary>
    public bool IsException { get; set; }
}
