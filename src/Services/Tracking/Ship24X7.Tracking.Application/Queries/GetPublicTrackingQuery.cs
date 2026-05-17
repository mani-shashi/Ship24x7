using MediatR;
using Ship24X7.Tracking.Application.DTOs;

namespace Ship24X7.Tracking.Application.Queries;

/// <summary>
/// Query for retrieving getpublictracking data. Defines query parameters and result type.
/// </summary>
public class GetPublicTrackingQuery : IRequest<PublicTrackingResponse?>
{
    /// <summary>
    /// Gets or sets the trackingNumber.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
}
