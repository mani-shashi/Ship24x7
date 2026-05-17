using Ship24X7.Shared.Domain;

namespace Ship24X7.Shipment.Domain.Events;

/// <summary>
/// Domain event raised when ShipmentCancelled occurs. Used for event-driven architecture and integration.
/// </summary>
public class ShipmentCancelled : BaseDomainEvent
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
    /// Gets or sets the customerId.
    /// </summary>
    public Guid CustomerId { get; set; }
    /// <summary>
    /// Gets or sets the cancellationreason.
    /// </summary>
    public string CancellationReason { get; set; } = string.Empty;
}
