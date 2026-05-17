using Ship24X7.Tracking.Domain.Enums;

namespace Ship24X7.Tracking.Application.DTOs;

/// <summary>
/// Data transfer object for PublicTracking data. Used for API responses and data serialization.
/// </summary>
public class PublicTrackingResponse
{
    /// <summary>
    /// Gets or sets the trackingNumber.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Current status. Null when the latest event has an unrecognised status string.
    /// </summary>
    public ShipmentStatus? CurrentStatus { get; set; }
    /// <summary>
    /// Gets or sets the estimateddeliverydate.
    /// </summary>
    public DateTime EstimatedDeliveryDate { get; set; }
    public List<PublicTrackingEventDto> Events { get; set; } = new();
}

/// <summary>
/// Data transfer object for PublicTrackingEventDto data. Used for API responses and data serialization.
/// </summary>
public class PublicTrackingEventDto
{
    /// <summary>
    /// Gets or sets the status. Null when the event has an unrecognised status string.
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
}
