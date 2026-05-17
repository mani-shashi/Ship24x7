using MediatR;
using Microsoft.Extensions.Logging;
using Ship24X7.Shipment.Application.Commands;
using Ship24X7.Shipment.Application.Interfaces;

namespace Ship24X7.Shipment.Application.Handlers;

/// <summary>
/// Handles status updates for shipments triggered by external events (e.g. payment captured,
/// pickup completed, hub scans). Validates the transition via the aggregate's state machine,
/// persists the change, and publishes a ShipmentStatusChanged domain event so downstream
/// services (Tracking, Notification) stay in sync.
///
/// When the new status is PickedUp, automatically dispatches AutoAssignHubCommand so the
/// system selects the nearest available hub without requiring manual operator action.
/// </summary>
public class UpdateShipmentStatusCommandHandler : IRequestHandler<UpdateShipmentStatusCommand, bool>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IShipmentEventPublisher _eventPublisher;
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateShipmentStatusCommandHandler> _logger;

    public UpdateShipmentStatusCommandHandler(
        IShipmentRepository shipmentRepository,
        IShipmentEventPublisher eventPublisher,
        IMediator mediator,
        ILogger<UpdateShipmentStatusCommandHandler> logger)
    {
        _shipmentRepository = shipmentRepository;
        _eventPublisher = eventPublisher;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateShipmentStatusCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
        if (aggregate == null)
        {
            _logger.LogWarning(
                "[UpdateShipmentStatus] Shipment {ShipmentId} not found", request.ShipmentId);
            return false;
        }

        try
        {
            _logger.LogInformation(
                "[UpdateShipmentStatus] Shipment {ShipmentId}: {OldStatus} → {NewStatus}. Reason: {Reason}",
                request.ShipmentId, aggregate.Shipment.Status, request.NewStatus, request.Reason);

            aggregate.UpdateStatus(request.NewStatus, request.Reason,
                request.ChangedBy == Guid.Empty ? null : request.ChangedBy);

            await _shipmentRepository.UpdateAsync(aggregate.Shipment);

            foreach (var domainEvent in aggregate.DomainEvents)
            {
                if (domainEvent is Ship24X7.Shipment.Domain.Events.ShipmentStatusChanged statusChanged)
                    await _eventPublisher.PublishAsync(statusChanged);
            }
            aggregate.ClearDomainEvents();

            // Auto-assign hub when shipment is picked up
            if (request.NewStatus == Domain.Enums.ShipmentStatus.PickedUp)
            {
                _ = _mediator.Send(new AutoAssignHubCommand
                {
                    ShipmentId  = aggregate.Shipment.Id,
                    SenderCity  = aggregate.Shipment.SenderAddress?.City  ?? string.Empty,
                    SenderState = aggregate.Shipment.SenderAddress?.State ?? string.Empty
                }, cancellationToken);
            }

            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                "[UpdateShipmentStatus] Invalid transition for Shipment {ShipmentId}: {Error}",
                request.ShipmentId, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[UpdateShipmentStatus] Unexpected error for Shipment {ShipmentId}", request.ShipmentId);
            return false;
        }
    }
}
