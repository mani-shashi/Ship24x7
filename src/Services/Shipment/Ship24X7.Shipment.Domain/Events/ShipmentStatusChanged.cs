using Ship24X7.Shared.Domain;
using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Domain.Events;

/// <summary>
/// Domain event raised when ShipmentStatusChanged occurs. Used for event-driven architecture and integration.
/// </summary>
public class ShipmentStatusChanged : BaseDomainEvent
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
    /// Gets or sets the new status (backing enum field; serialized as string via JsonStringEnumConverter).
    /// </summary>
    public ShipmentStatus NewStatus { get; set; }
    /// <summary>
    /// Gets or sets the old status (backing enum field; kept for internal use).
    /// </summary>
    public ShipmentStatus OldStatus { get; set; }
    /// <summary>
    /// Contract alias for <see cref="OldStatus"/> — emitted as a string in the JSON event payload.
    /// </summary>
    public string PreviousStatus => OldStatus.ToString();
    /// <summary>
    /// Contract alias for <see cref="NewStatus"/> — emitted as a string in the JSON event payload.
    /// </summary>
    public string Status => NewStatus.ToString();
    /// <summary>
    /// Gets or sets the reason.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the physical location where the event occurred (hub name, city, etc.).
    /// Used by the Tracking Service to populate the location field on the TrackingEvent.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Event schema version for forward-compatibility. Consumers should check this field
    /// before processing to detect breaking changes.
    /// </summary>
    public string SchemaVersion { get; set; } = "1.0";
}
