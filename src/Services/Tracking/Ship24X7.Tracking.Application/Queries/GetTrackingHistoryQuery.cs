using MediatR;
using Ship24X7.Tracking.Application.DTOs;

namespace Ship24X7.Tracking.Application.Queries;

/// <summary>
/// Query for retrieving gettrackinghistory data. Defines query parameters and result type.
/// </summary>
public class GetTrackingHistoryQuery : IRequest<TrackingHistoryResponse>
{
    /// <summary>
    /// Gets or sets the trackingNumber.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the customerId.
    /// </summary>
    public Guid? CustomerId { get; set; } // For authorization
}
