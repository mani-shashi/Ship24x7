using MediatR;
using Microsoft.Extensions.Logging;
using Ship24X7.Shipment.Application.Commands;
using Ship24X7.Shipment.Application.Interfaces;

namespace Ship24X7.Shipment.Application.Handlers;

/// <summary>
/// Handles pickup completion.
///
/// Flow:
///   1. Fetch the Pickup by ID to get the ShipmentId.
///   2. Load the ShipmentAggregate and call CompletePickup — transitions
///      the shipment to PickedUp and raises domain events.
///   3. Persist the updated shipment.
///   4. Publish ShipmentStatusChanged so Tracking + Notification react.
///   5. Immediately dispatch AutoAssignHubCommand so the shipment is
///      assigned to the nearest available hub without manual intervention.
/// </summary>
public class CompletePickupCommandHandler : IRequestHandler<CompletePickupCommand, bool>
{
    private readonly IPickupRepository _pickupRepository;
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IShipmentEventPublisher _eventPublisher;
    private readonly IMediator _mediator;
    private readonly ILogger<CompletePickupCommandHandler> _logger;

    public CompletePickupCommandHandler(
        IPickupRepository pickupRepository,
        IShipmentRepository shipmentRepository,
        IShipmentEventPublisher eventPublisher,
        IMediator mediator,
        ILogger<CompletePickupCommandHandler> logger)
    {
        _pickupRepository = pickupRepository;
        _shipmentRepository = shipmentRepository;
        _eventPublisher = eventPublisher;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<bool> Handle(CompletePickupCommand request, CancellationToken cancellationToken)
    {
        // 1. Resolve the shipment from the pickup
        var pickup = await _pickupRepository.GetByIdAsync(request.PickupId);
        if (pickup == null)
            throw new InvalidOperationException($"Pickup {request.PickupId} not found");

        var aggregate = await _shipmentRepository.GetByIdAsync(pickup.ShipmentId);
        if (aggregate == null)
            throw new InvalidOperationException($"Shipment for pickup {request.PickupId} not found");

        // 2. Complete the pickup — transitions shipment to PickedUp
        aggregate.CompletePickup(request.DriverId);

        // 3. Persist
        await _shipmentRepository.UpdateAsync(aggregate.Shipment);

        // 4. Publish domain events (ShipmentStatusChanged → Tracking + Notification)
        foreach (var evt in aggregate.DomainEvents)
        {
            if (evt is Domain.Events.ShipmentStatusChanged sc)
                await _eventPublisher.PublishAsync(sc);
            else if (evt is Domain.Events.PickupCompleted pc)
                await _eventPublisher.PublishAsync(pc);
        }
        aggregate.ClearDomainEvents();

        _logger.LogInformation(
            "[CompletePickup] Shipment {ShipmentId} → PickedUp. Driver: {DriverId}",
            aggregate.Shipment.Id, request.DriverId);

        // 5. Auto-assign hub immediately — no manual intervention needed
        await _mediator.Send(new AutoAssignHubCommand
        {
            ShipmentId  = aggregate.Shipment.Id,
            SenderCity  = aggregate.Shipment.SenderAddress?.City  ?? string.Empty,
            SenderState = aggregate.Shipment.SenderAddress?.State ?? string.Empty
        }, cancellationToken);

        return true;
    }
}
