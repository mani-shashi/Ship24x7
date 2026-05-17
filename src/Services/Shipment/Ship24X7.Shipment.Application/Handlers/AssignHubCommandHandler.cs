using MediatR;
using Microsoft.Extensions.Logging;
using Ship24X7.Shipment.Application.Commands;
using Ship24X7.Shipment.Application.Interfaces;

namespace Ship24X7.Shipment.Application.Handlers;

/// <summary>
/// Handles hub assignment for a shipment.
/// Validates hub exists and is active, updates CurrentHubId, adjusts hub load counters,
/// and records an audit history entry without changing shipment status.
/// </summary>
public class AssignHubCommandHandler : IRequestHandler<AssignHubCommand, AssignHubResult>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IHubRepository _hubRepository;
    private readonly IShipmentEventPublisher _eventPublisher;
    private readonly ILogger<AssignHubCommandHandler> _logger;

    public AssignHubCommandHandler(
        IShipmentRepository shipmentRepository,
        IHubRepository hubRepository,
        IShipmentEventPublisher eventPublisher,
        ILogger<AssignHubCommandHandler> logger)
    {
        _shipmentRepository = shipmentRepository;
        _hubRepository = hubRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<AssignHubResult> Handle(AssignHubCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
        if (aggregate == null)
            return new AssignHubResult { Success = false, Message = "Shipment not found" };

        var hub = await _hubRepository.GetByIdAsync(request.HubId);
        if (hub == null || !hub.IsActive)
            return new AssignHubResult { Success = false, Message = "Hub not found or inactive" };

        try
        {
            // Decrement load on the previous hub if there was one
            if (aggregate.Shipment.CurrentHubId.HasValue
                && aggregate.Shipment.CurrentHubId != request.HubId)
            {
                var previousHub = await _hubRepository.GetByIdAsync(aggregate.Shipment.CurrentHubId.Value);
                if (previousHub != null && previousHub.CurrentLoad > 0)
                {
                    previousHub.CurrentLoad--;
                    await _hubRepository.UpdateAsync(previousHub);
                }
            }

            // Assign to new hub
            aggregate.AssignToHub(request.HubId, hub.Name, request.OperatorId, request.Reason);

            // Increment new hub load
            hub.CurrentLoad++;
            await _hubRepository.UpdateAsync(hub);

            await _shipmentRepository.UpdateAsync(aggregate.Shipment);

            // Publish location event so Tracking Service records it
            foreach (var evt in aggregate.DomainEvents)
            {
                if (evt is Domain.Events.ShipmentStatusChanged sc)
                    await _eventPublisher.PublishAsync(sc);
            }
            aggregate.ClearDomainEvents();

            _logger.LogInformation(
                "[AssignHub] Shipment {ShipmentId} assigned to hub {HubName} ({HubId}) by operator {OperatorId}",
                request.ShipmentId, hub.Name, request.HubId, request.OperatorId);

            return new AssignHubResult
            {
                Success = true,
                HubName = hub.Name,
                Message = $"Shipment assigned to {hub.Name}"
            };
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("[AssignHub] {Error}", ex.Message);
            return new AssignHubResult { Success = false, Message = ex.Message };
        }
    }
}
