using Ship24X7.Shared.Domain;

namespace Ship24X7.Tracking.Domain.Events;

/// <summary>
/// Domain event raised when ShipmentDelivered occurs. Used for event-driven architecture and integration.
/// </summary>
public class ShipmentDelivered : BaseDomainEvent
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
    /// Gets or sets the receivedby.
    /// </summary>
    public string ReceivedBy { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the deliverydate.
    /// </summary>
    public DateTime DeliveryDate { get; set; }
    /// <summary>
    /// Gets or sets the latitude.
    /// </summary>
    public decimal Latitude { get; set; }
    /// <summary>
    /// Gets or sets the longitude.
    /// </summary>
    public decimal Longitude { get; set; }
    /// <summary>
    /// Gets or sets the deliveredby.
    /// </summary>
    public Guid DeliveredBy { get; set; }
}
