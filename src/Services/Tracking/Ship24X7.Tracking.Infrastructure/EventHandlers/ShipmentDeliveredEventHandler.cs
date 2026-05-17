using MediatR;
using Microsoft.Extensions.Logging;
using Ship24X7.Tracking.Domain.Events;
using Ship24X7.Tracking.Infrastructure.Messaging;

namespace Ship24X7.Tracking.Infrastructure.EventHandlers;

/// <summary>
/// Handler for processing ShipmentDeliveredEventHandler requests. Implements business logic and coordinates with repositories and services.
/// </summary>
public class ShipmentDeliveredEventHandler : INotificationHandler<ShipmentDelivered>
{
    private readonly TrackingEventPublisher _eventPublisher;
    private readonly ILogger<ShipmentDeliveredEventHandler> _logger;

    public ShipmentDeliveredEventHandler(
        TrackingEventPublisher eventPublisher,
        ILogger<ShipmentDeliveredEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public Task Handle(ShipmentDelivered notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing ShipmentDelivered event for tracking number {TrackingNumber} with CorrelationId {CorrelationId}",
            notification.TrackingNumber, notification.CorrelationId);

        _eventPublisher.PublishEvent(notification, "shipment.delivered");

        return Task.CompletedTask;
    }
}
