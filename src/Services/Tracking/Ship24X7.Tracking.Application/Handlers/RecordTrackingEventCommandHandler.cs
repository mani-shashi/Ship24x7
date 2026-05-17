using MediatR;
using Ship24X7.Tracking.Application.Commands;
using Ship24X7.Tracking.Application.DTOs;
using Ship24X7.Tracking.Application.Interfaces;
using Ship24X7.Tracking.Domain.Entities;
using Ship24X7.Tracking.Domain.Enums;
using Ship24X7.Tracking.Domain.Events;

namespace Ship24X7.Tracking.Application.Handlers;

/// <summary>
/// Handles tracking event recording for shipment status updates, location changes, and exception events.
/// Publishes domain events for notifications and triggers ShipmentDelayed event for exception scenarios.
/// </summary>
public class RecordTrackingEventCommandHandler : IRequestHandler<RecordTrackingEventCommand, TrackingEventResponse>
{
    private readonly ITrackingEventRepository _trackingEventRepository;
    private readonly IPublisher _publisher;

    /// <summary>
    /// Initializes a new instance of the RecordTrackingEventCommandHandler class.
    /// </summary>
    /// <param name="trackingEventRepository">Repository for tracking event operations.</param>
    /// <param name="publisher">MediatR publisher for domain events.</param>
    public RecordTrackingEventCommandHandler(
        ITrackingEventRepository trackingEventRepository,
        IPublisher publisher)
    {
        _trackingEventRepository = trackingEventRepository;
        _publisher = publisher;
    }

    /// <summary>
    /// Records a new tracking event for shipment status updates and location changes.
    /// Process flow:
    /// 1. Creates TrackingEvent entity with shipment ID, tracking number, status, description, location, timestamp
    /// 2. Stores exception flag and reason if event represents a delay or problem
    /// 3. Records who created the event (system, driver, hub staff)
    /// 4. Persists tracking event to database
    /// 5. Publishes TrackingEventRecorded domain event for real-time notifications
    /// 6. If event is an exception during InTransit, PickedUp, or OutForDelivery status:
    ///    a. Publishes ShipmentDelayed domain event
    ///    b. Includes current status, exception reason, location, and delay timestamp
    ///    c. Triggers customer notifications and escalation workflows
    /// 7. Returns tracking event response with all event details
    /// Exception events trigger additional processing for customer service and delay management.
    /// </summary>
    /// <param name="request">Record tracking event command with shipment ID, tracking number, status, description, location, timestamp, exception flag, and recorder ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Tracking event response with ID, shipment details, status, description, location, timestamp, and exception information.</returns>
    public async Task<TrackingEventResponse> Handle(RecordTrackingEventCommand request, CancellationToken cancellationToken)
    {
        var trackingEvent = new TrackingEvent
        {
            Id = Guid.NewGuid(),
            ShipmentId = request.ShipmentId,
            TrackingNumber = request.TrackingNumber,
            Status = request.Status,
            Description = request.Description,
            Location = request.Location,
            EventTimestamp = request.EventTimestamp,
            IsException = request.IsException,
            ExceptionReason = request.ExceptionReason,
            RecordedBy = request.RecordedBy,
            CreatedAt = DateTime.UtcNow
        };

        await _trackingEventRepository.AddAsync(trackingEvent, cancellationToken);

        // Publish domain event
        var domainEvent = new TrackingEventRecorded
        {
            ShipmentId = trackingEvent.ShipmentId,
            TrackingNumber = trackingEvent.TrackingNumber,
            Status = trackingEvent.Status,
            Description = trackingEvent.Description,
            Location = trackingEvent.Location,
            EventTimestamp = trackingEvent.EventTimestamp,
            IsException = trackingEvent.IsException
        };
        await _publisher.Publish(domainEvent, cancellationToken);

        // If exception, publish ShipmentDelayed event
        if (trackingEvent.IsException && 
            (trackingEvent.Status == ShipmentStatus.InTransit || 
             trackingEvent.Status == ShipmentStatus.PickedUp || 
             trackingEvent.Status == ShipmentStatus.OutForDelivery))
        {
            var delayedEvent = new ShipmentDelayed
            {
                ShipmentId = trackingEvent.ShipmentId,
                TrackingNumber = trackingEvent.TrackingNumber,
                CurrentStatus = trackingEvent.Status,
                ExceptionReason = trackingEvent.ExceptionReason ?? "Unknown delay",
                Location = trackingEvent.Location,
                DelayedAt = trackingEvent.EventTimestamp
            };
            await _publisher.Publish(delayedEvent, cancellationToken);
        }

        return MapToResponse(trackingEvent);
    }

    /// <summary>
    /// Maps tracking event entity to response DTO.
    /// </summary>
    /// <param name="trackingEvent">Tracking event entity from database.</param>
    /// <returns>Tracking event response DTO for API response.</returns>
    private TrackingEventResponse MapToResponse(TrackingEvent trackingEvent)
    {
        return new TrackingEventResponse
        {
            Id = trackingEvent.Id,
            ShipmentId = trackingEvent.ShipmentId,
            TrackingNumber = trackingEvent.TrackingNumber,
            Status = trackingEvent.Status,
            Description = trackingEvent.Description,
            Location = trackingEvent.Location,
            EventTimestamp = trackingEvent.EventTimestamp,
            IsException = trackingEvent.IsException,
            ExceptionReason = trackingEvent.ExceptionReason
        };
    }
}
