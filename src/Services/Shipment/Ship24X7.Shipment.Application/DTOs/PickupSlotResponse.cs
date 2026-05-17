using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Application.DTOs;

/// <summary>
/// Data transfer object for PickupSlot data. Used for API responses and data serialization.
/// </summary>
public class PickupSlotResponse
{
    /// <summary>
    /// Gets or sets the timeslot.
    /// </summary>
    public PickupTimeSlot TimeSlot { get; set; }
    /// <summary>
    /// Gets or sets the timerange.
    /// </summary>
    public string TimeRange { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the isavailable.
    /// </summary>
    public bool IsAvailable { get; set; }
}
