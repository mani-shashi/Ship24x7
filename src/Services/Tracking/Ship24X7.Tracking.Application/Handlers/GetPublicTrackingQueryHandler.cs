using MediatR;
using Ship24X7.Tracking.Application.DTOs;
using Ship24X7.Tracking.Application.Interfaces;
using Ship24X7.Tracking.Application.Queries;

namespace Ship24X7.Tracking.Application.Handlers;

/// <summary>
/// Query for retrieving getpublictrackinghandler data. Defines query parameters and result type.
/// </summary>
public class GetPublicTrackingQueryHandler : IRequestHandler<GetPublicTrackingQuery, PublicTrackingResponse?>
{
    private readonly ITrackingEventRepository _trackingEventRepository;

    public GetPublicTrackingQueryHandler(ITrackingEventRepository trackingEventRepository)
    {
        _trackingEventRepository = trackingEventRepository;
    }

    public async Task<PublicTrackingResponse?> Handle(GetPublicTrackingQuery request, CancellationToken cancellationToken)
    {
        var events = await _trackingEventRepository.GetByTrackingNumberAsync(request.TrackingNumber, cancellationToken);
        
        if (!events.Any())
        {
            return null;
        }

        var orderedEvents = events.OrderByDescending(e => e.EventTimestamp).ToList();
        var latestEvent = orderedEvents.First();

        // Return limited information for public access (no personal details)
        return new PublicTrackingResponse
        {
            TrackingNumber = request.TrackingNumber,
            CurrentStatus = latestEvent.Status,
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3), // TODO: Get from shipment
            Events = orderedEvents.Select(e => new PublicTrackingEventDto
            {
                Status = e.Status,
                Description = e.Description,
                Location = e.Location,
                EventTimestamp = e.EventTimestamp
            }).ToList()
        };
    }
}
