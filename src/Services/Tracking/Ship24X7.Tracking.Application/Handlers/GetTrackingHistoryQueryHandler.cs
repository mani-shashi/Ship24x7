using MediatR;
using Ship24X7.Tracking.Application.DTOs;
using Ship24X7.Tracking.Application.Interfaces;
using Ship24X7.Tracking.Application.Queries;

namespace Ship24X7.Tracking.Application.Handlers;

/// <summary>
/// Query for retrieving gettrackinghistoryhandler data. Defines query parameters and result type.
/// </summary>
public class GetTrackingHistoryQueryHandler : IRequestHandler<GetTrackingHistoryQuery, TrackingHistoryResponse>
{
    private readonly ITrackingEventRepository _trackingEventRepository;

    public GetTrackingHistoryQueryHandler(ITrackingEventRepository trackingEventRepository)
    {
        _trackingEventRepository = trackingEventRepository;
    }

    public async Task<TrackingHistoryResponse> Handle(GetTrackingHistoryQuery request, CancellationToken cancellationToken)
    {
        var events = await _trackingEventRepository.GetByTrackingNumberAsync(request.TrackingNumber, cancellationToken);
        
        if (!events.Any())
        {
            throw new InvalidOperationException($"No tracking events found for tracking number {request.TrackingNumber}");
        }

        var orderedEvents = events.OrderByDescending(e => e.EventTimestamp).ToList();
        var latestEvent = orderedEvents.First();

        return new TrackingHistoryResponse
        {
            TrackingNumber = request.TrackingNumber,
            CurrentStatus = latestEvent.Status,
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3), // TODO: Get from shipment
            Events = orderedEvents.Select(e => new TrackingEventResponse
            {
                Id = e.Id,
                ShipmentId = e.ShipmentId,
                TrackingNumber = e.TrackingNumber,
                Status = e.Status,
                Description = e.Description,
                Location = e.Location,
                EventTimestamp = e.EventTimestamp,
                IsException = e.IsException,
                ExceptionReason = e.ExceptionReason
            }).ToList()
        };
    }
}
