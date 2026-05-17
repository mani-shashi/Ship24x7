using MediatR;
using Microsoft.Extensions.Logging;
using Ship24X7.Shipment.Application.Commands;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Domain.Events;

namespace Ship24X7.Shipment.Application.Handlers;

/// <summary>
/// Handles InTransit/Delayed → OutForDelivery transition.
/// Generates a 6-digit OTP, stores its SHA-256 hash on the shipment,
/// and publishes an OutForDeliveryEvent carrying the raw OTP so the
/// Notification Service can SMS/email it to the customer.
/// </summary>
public class MarkOutForDeliveryCommandHandler : IRequestHandler<MarkOutForDeliveryCommand, MarkOutForDeliveryResult>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IHubRepository _hubRepository;
    private readonly IShipmentEventPublisher _eventPublisher;
    private readonly ILogger<MarkOutForDeliveryCommandHandler> _logger;

    public MarkOutForDeliveryCommandHandler(
        IShipmentRepository shipmentRepository,
        IHubRepository hubRepository,
        IShipmentEventPublisher eventPublisher,
        ILogger<MarkOutForDeliveryCommandHandler> logger)
    {
        _shipmentRepository = shipmentRepository;
        _hubRepository = hubRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<MarkOutForDeliveryResult> Handle(
        MarkOutForDeliveryCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
        if (aggregate == null)
            return new MarkOutForDeliveryResult { Success = false, Message = "Shipment not found" };

        var hub = await _hubRepository.GetByIdAsync(request.HubId);
        if (hub == null || !hub.IsActive)
            return new MarkOutForDeliveryResult { Success = false, Message = "Hub not found or inactive" };

        try
        {
            // Aggregate generates OTP, hashes it, stores hash, returns raw OTP
            var rawOtp = aggregate.MarkOutForDelivery(
                request.DeliveryAgentId, request.OperatorId, hub.Name);

            await _shipmentRepository.UpdateAsync(aggregate.Shipment);

            // Publish status changed event (for Tracking Service)
            foreach (var evt in aggregate.DomainEvents)
            {
                if (evt is ShipmentStatusChanged sc)
                    await _eventPublisher.PublishAsync(sc);
            }

            // Publish dedicated OTP event so Notification Service sends it to the customer
            await _eventPublisher.PublishAsync(new ShipmentOutForDelivery
            {
                ShipmentId = aggregate.Shipment.Id,
                TrackingNumber = aggregate.Shipment.TrackingNumber,
                CustomerId = aggregate.Shipment.CustomerId,
                DeliveryAgentId = request.DeliveryAgentId,
                DeliveryOtp = rawOtp,           // raw OTP — Notification Service sends this
                OtpExpiresAt = aggregate.Shipment.OtpExpiresAt!.Value,
                CorrelationId = aggregate.Shipment.CorrelationId
            });

            aggregate.ClearDomainEvents();

            _logger.LogInformation(
                "[MarkOutForDelivery] Shipment {Id} → OutForDelivery. Agent: {Agent}. OTP dispatched.",
                request.ShipmentId, request.DeliveryAgentId);

            return new MarkOutForDeliveryResult
            {
                Success = true,
                Message = "Shipment marked out for delivery. OTP sent to customer.",
                RawOtp = rawOtp   // returned to caller for operational confirmation only
            };
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("[MarkOutForDelivery] {Error}", ex.Message);
            return new MarkOutForDeliveryResult { Success = false, Message = ex.Message };
        }
    }
}
