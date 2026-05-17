using Ship24X7.Tracking.Domain.Enums;

namespace Ship24X7.Tracking.Application.DTOs;

/// <summary>
/// Data transfer object for TrackingEvent data. Used for API responses and data serialization.
/// </summary>
public class TrackingEventResponse
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets the shipmentId.
    /// </summary>
    public Guid ShipmentId { get; set; }
    /// <summary>
    /// Gets or sets the trackingNumber.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the status. Null when the event was recorded with an unrecognised status string (see RawStatus).
    /// </summary>
    public ShipmentStatus? Status { get; set; }

    /// <summary>
    /// Gets or sets the raw status string when Status is null (unrecognised value received from the event bus).
    /// </summary>
    public string? RawStatus { get; set; }
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
    /// <summary>
    /// Gets or sets the exceptionreason.
    /// </summary>
    public string? ExceptionReason { get; set; }
}
