using Ship24X7.Shared.Domain;
using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Domain.Entities;

/// <summary>
/// Domain entity representing a scheduled pickup for shipment collection.
/// Contains pickup date, time slot, confirmation number, driver assignment, and completion status.
/// Manages the first-mile logistics of collecting packages from senders.
/// </summary>
public class Pickup : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the pickup record.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the shipment ID this pickup is scheduled for.
    /// Links pickup to specific shipment booking.
    /// </summary>
    public Guid ShipmentId { get; set; }
    
    /// <summary>
    /// Gets or sets the unique confirmation number for the pickup.
    /// Format: PUYYYYMMDDXXXXXX (e.g., PU20260420A1B2C3).
    /// Used by customers and drivers to identify and verify pickup appointments.
    /// </summary>
    public string ConfirmationNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the scheduled pickup date.
    /// Date when driver will visit sender's location to collect the shipment.
    /// </summary>
    public DateTime PickupDate { get; set; }
    
    /// <summary>
    /// Gets or sets the time slot for pickup.
    /// Defines pickup window: Morning (09:00-12:00), Afternoon (12:00-16:00), Evening (16:00-19:00), Night (19:00-21:00).
    /// </summary>
    public PickupTimeSlot TimeSlot { get; set; }
    
    /// <summary>
    /// Gets or sets the current status of the pickup.
    /// Tracks progression: Scheduled → EnRoute → Completed/Failed/Cancelled.
    /// </summary>
    public PickupStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets the driver ID assigned to perform this pickup.
    /// Null until pickup is assigned to a driver for route optimization.
    /// </summary>
    public Guid? AssignedDriverId { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when pickup was completed.
    /// Null until driver successfully collects the shipment and marks pickup as complete.
    /// </summary>
    public DateTime? CompletedAt { get; set; }
    
    /// <summary>
    /// Gets or sets optional notes about the pickup.
    /// Can include special instructions, access codes, landmark details, or completion remarks.
    /// </summary>
    public string Notes { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the shipment navigation property.
    /// Contains complete shipment details including sender address and items to collect.
    /// </summary>
    public Shipment Shipment { get; set; } = null!;
}
