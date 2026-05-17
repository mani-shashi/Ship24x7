using MediatR;
using Ship24X7.Tracking.Application.DTOs;
using Ship24X7.Tracking.Domain.Enums;

namespace Ship24X7.Tracking.Application.Commands;

/// <summary>
/// Command for recordtrackingevent operation. Encapsulates request data and validation rules.
/// </summary>
public class RecordTrackingEventCommand : IRequest<TrackingEventResponse>
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
    /// Gets or sets the status.
    /// </summary>
    public ShipmentStatus Status { get; set; }
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
    /// <summary>
    /// Gets or sets the recordedby.
    /// </summary>
    public string? RecordedBy { get; set; }
}
