using MediatR;
using Microsoft.Extensions.Logging;
using Ship24X7.Shipment.Application.Commands;
using Ship24X7.Shipment.Application.Interfaces;

namespace Ship24X7.Shipment.Application.Handlers;

/// <summary>
/// Handles OutForDelivery → Delivered transition.
/// Validates the OTP provided by the delivery agent.
/// Supports supervisor override (caller must verify role before setting SupervisorOverride = true).
/// </summary>
public class CompleteDeliveryCommandHandler : IRequestHandler<CompleteDeliveryCommand, bool>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IShipmentEventPublisher _eventPublisher;
    private readonly ILogger<CompleteDeliveryCommandHandler> _logger;

    public CompleteDeliveryCommandHandler(
        IShipmentRepository shipmentRepository,
        IShipmentEventPublisher eventPublisher,
        ILogger<CompleteDeliveryCommandHandler> logger)
    {
        _shipmentRepository = shipmentRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<bool> Handle(CompleteDeliveryCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
        if (aggregate == null)
        {
            _logger.LogWarning("[CompleteDelivery] Shipment {Id} not found", request.ShipmentId);
            return false;
        }

        try
        {
            aggregate.CompleteDelivery(request.Otp, request.OperatorId, request.SupervisorOverride);
            await _shipmentRepository.UpdateAsync(aggregate.Shipment);

            foreach (var evt in aggregate.DomainEvents)
            {
                if (evt is Domain.Events.ShipmentStatusChanged sc)
                    await _eventPublisher.PublishAsync(sc);
            }
            aggregate.ClearDomainEvents();

            _logger.LogInformation(
                "[CompleteDelivery] Shipment {Id} → Delivered. Override={Override}",
                request.ShipmentId, request.SupervisorOverride);

            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("[CompleteDelivery] {Error}", ex.Message);
            throw; // Controller returns 400 with the message
        }
    }
}
