using MediatR;
using Microsoft.Extensions.Logging;
using Ship24X7.Shipment.Application.Commands;
using Ship24X7.Shipment.Application.Interfaces;

namespace Ship24X7.Shipment.Application.Handlers;

/// <summary>
/// Automatically assigns the most suitable hub to a shipment that just reached PickedUp status.
///
/// Selection algorithm (in priority order):
///   1. Active hub in the same city with available capacity
///   2. Active hub in the same state with available capacity
///   3. Any active hub with available capacity (national fallback)
///   4. No assignment — operator must assign manually
///
/// "Available capacity" means CurrentLoad &lt; Capacity.
/// </summary>
public class AutoAssignHubCommandHandler : IRequestHandler<AutoAssignHubCommand, AutoAssignHubResult>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IHubRepository _hubRepository;
    private readonly IShipmentEventPublisher _eventPublisher;
    private readonly ILogger<AutoAssignHubCommandHandler> _logger;

    public AutoAssignHubCommandHandler(
        IShipmentRepository shipmentRepository,
        IHubRepository hubRepository,
        IShipmentEventPublisher eventPublisher,
        ILogger<AutoAssignHubCommandHandler> logger)
    {
        _shipmentRepository = shipmentRepository;
        _hubRepository = hubRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<AutoAssignHubResult> Handle(
        AutoAssignHubCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
        if (aggregate == null)
        {
            _logger.LogWarning("[AutoAssignHub] Shipment {Id} not found", request.ShipmentId);
            return new AutoAssignHubResult { Assigned = false };
        }

        // Already has a hub — nothing to do
        if (aggregate.Shipment.CurrentHubId.HasValue)
            return new AutoAssignHubResult
            {
                Assigned = true,
                HubId    = aggregate.Shipment.CurrentHubId,
                HubName  = "Already assigned"
            };

        var allHubs = await _hubRepository.GetAllAsync(activeOnly: true);
        var availableHubs = allHubs.Where(h => h.CurrentLoad < h.Capacity).ToList();

        if (!availableHubs.Any())
        {
            _logger.LogWarning(
                "[AutoAssignHub] No hubs with available capacity for Shipment {Id}", request.ShipmentId);
            return new AutoAssignHubResult { Assigned = false };
        }

        // Priority 1: same city
        var hub = availableHubs.FirstOrDefault(h =>
            string.Equals(h.City, request.SenderCity, StringComparison.OrdinalIgnoreCase));

        // Priority 2: same state
        hub ??= availableHubs.FirstOrDefault(h =>
            string.Equals(h.State, request.SenderState, StringComparison.OrdinalIgnoreCase));

        // Priority 3: any available hub
        hub ??= availableHubs.First();

        // Assign
        aggregate.AssignToHub(hub.Id, hub.Name, Guid.Empty, "Auto-assigned on pickup");
        hub.CurrentLoad++;

        await _hubRepository.UpdateAsync(hub);
        await _shipmentRepository.UpdateAsync(aggregate.Shipment);

        foreach (var evt in aggregate.DomainEvents)
        {
            if (evt is Domain.Events.ShipmentStatusChanged sc)
                await _eventPublisher.PublishAsync(sc);
        }
        aggregate.ClearDomainEvents();

        _logger.LogInformation(
            "[AutoAssignHub] Shipment {Id} auto-assigned to hub {HubName} ({HubId})",
            request.ShipmentId, hub.Name, hub.Id);

        return new AutoAssignHubResult { Assigned = true, HubId = hub.Id, HubName = hub.Name };
    }
}
