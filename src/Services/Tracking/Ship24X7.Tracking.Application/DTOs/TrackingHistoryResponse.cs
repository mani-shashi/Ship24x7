using Ship24X7.Tracking.Domain.Enums;

namespace Ship24X7.Tracking.Application.DTOs;

/// <summary>
/// Data transfer object for TrackingHistory data. Used for API responses and data serialization.
/// </summary>
public class TrackingHistoryResponse
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
    public List<TrackingEventResponse> Events { get; set; } = new();
}
