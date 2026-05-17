using MediatR;
using Microsoft.Extensions.Logging;
using Ship24X7.Tracking.Domain.Events;
using Ship24X7.Tracking.Infrastructure.Messaging;

namespace Ship24X7.Tracking.Infrastructure.EventHandlers;

/// <summary>
/// Handler for processing ShipmentDelayedEventHandler requests. Implements business logic and coordinates with repositories and services.
/// </summary>
public class ShipmentDelayedEventHandler : INotificationHandler<ShipmentDelayed>
{
    private readonly TrackingEventPublisher _eventPublisher;
    private readonly ILogger<ShipmentDelayedEventHandler> _logger;

    public ShipmentDelayedEventHandler(
        TrackingEventPublisher eventPublisher,
        ILogger<ShipmentDelayedEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public Task Handle(ShipmentDelayed notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing ShipmentDelayed event for tracking number {TrackingNumber} with CorrelationId {CorrelationId}",
            notification.TrackingNumber, notification.CorrelationId);

        _eventPublisher.PublishEvent(notification, "shipment.delayed");

        return Task.CompletedTask;
    }
}
