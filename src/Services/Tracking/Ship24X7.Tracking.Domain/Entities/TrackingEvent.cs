using Ship24X7.Shared.Domain;
using Ship24X7.Tracking.Domain.Enums;

namespace Ship24X7.Tracking.Domain.Entities;

/// <summary>
/// Domain entity representing a tracking event in the shipment lifecycle.
/// Records status changes, location updates, timestamps, and exception information for shipment tracking history.
/// Provides complete audit trail of shipment journey from pickup to delivery.
/// </summary>
public class TrackingEvent : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the tracking event.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the shipment ID this tracking event belongs to.
    /// Links event to internal shipment record.
    /// </summary>
    public Guid ShipmentId { get; set; }
    
    /// <summary>
    /// Gets or sets the tracking number for public shipment identification.
    /// Enables customer tracking without exposing internal shipment ID.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the shipment status at this event.
    /// Represents current state in shipment lifecycle (e.g., PickedUp, InTransit, OutForDelivery, Delivered).
    /// Null when the status string from the event could not be mapped to a known enum value.
    /// </summary>
    public ShipmentStatus? Status { get; set; }

    /// <summary>
    /// Gets or sets the raw status string received from the event when it cannot be mapped to a known <see cref="ShipmentStatus"/> value.
    /// Populated only when <see cref="Status"/> is null, preserving the original unrecognised status for diagnostics.
    /// </summary>
    public string? RawStatus { get; set; }
    
    /// <summary>
    /// Gets or sets the human-readable description of the tracking event.
    /// Provides context for customers (e.g., "Package picked up from sender", "Arrived at sorting facility").
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the location where the event occurred.
    /// Can be facility name, city, GPS coordinates, or address depending on event type.
    /// </summary>
    public string Location { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the timestamp when the event occurred.
    /// Represents actual event time, not when it was recorded in the system.
    /// </summary>
    public DateTime EventTimestamp { get; set; }
    
    /// <summary>
    /// Gets or sets whether this event represents an exception or delay.
    /// True for problems like missed delivery, damaged package, customs hold, weather delay.
    /// </summary>
    public bool IsException { get; set; }
    
    /// <summary>
    /// Gets or sets the reason for the exception if IsException is true.
    /// Provides details about delay or problem (e.g., "Recipient not available", "Weather delay", "Customs inspection").
    /// </summary>
    public string? ExceptionReason { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of who recorded this event.
    /// Can be system ID, driver ID, hub staff ID, or external system identifier.
    /// </summary>
    public string? RecordedBy { get; set; }
}
