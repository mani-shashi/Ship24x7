using MediatR;
using Microsoft.Extensions.Logging;
using Ship24X7.Shipment.Application.Commands;
using Ship24X7.Shipment.Application.Interfaces;

namespace Ship24X7.Shipment.Application.Handlers;

/// <summary>
/// Handles PickedUp → InTransit transition.
/// Validates that the shipment has a CurrentHubId set before allowing transit —
/// enforcing the business rule that a package must be physically at a hub first.
/// </summary>
public class InitiateTransitCommandHandler : IRequestHandler<InitiateTransitCommand, bool>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IHubRepository _hubRepository;
    private readonly IShipmentEventPublisher _eventPublisher;
    private readonly ILogger<InitiateTransitCommandHandler> _logger;

    public InitiateTransitCommandHandler(
        IShipmentRepository shipmentRepository,
        IHubRepository hubRepository,
        IShipmentEventPublisher eventPublisher,
        ILogger<InitiateTransitCommandHandler> logger)
    {
        _shipmentRepository = shipmentRepository;
        _hubRepository = hubRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<bool> Handle(InitiateTransitCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
        if (aggregate == null)
        {
            _logger.LogWarning("[InitiateTransit] Shipment {Id} not found", request.ShipmentId);
            return false;
        }

        var hub = await _hubRepository.GetByIdAsync(request.HubId);
        if (hub == null || !hub.IsActive)
        {
            _logger.LogWarning("[InitiateTransit] Hub {HubId} not found or inactive", request.HubId);
            return false;
        }

        try
        {
            aggregate.InitiateTransit(request.HubId, hub.Name, request.OperatorId);
            await _shipmentRepository.UpdateAsync(aggregate.Shipment);

            foreach (var evt in aggregate.DomainEvents)
            {
                if (evt is Domain.Events.ShipmentStatusChanged sc)
                    await _eventPublisher.PublishAsync(sc);
            }
            aggregate.ClearDomainEvents();

            _logger.LogInformation(
                "[InitiateTransit] Shipment {Id} → InTransit from hub {HubName}",
                request.ShipmentId, hub.Name);

            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("[InitiateTransit] {Error}", ex.Message);
            throw; // Re-throw so controller returns 400 with the message
        }
    }
}
