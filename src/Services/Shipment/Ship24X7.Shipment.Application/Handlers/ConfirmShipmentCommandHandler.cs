using MediatR;
using Ship24X7.Shipment.Application.Commands;
using Ship24X7.Shipment.Application.DTOs;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Domain.Events;

namespace Ship24X7.Shipment.Application.Handlers;

/// <summary>
/// Handler for processing ConfirmShipment requests. Implements business logic and coordinates with repositories and services.
/// </summary>
public class ConfirmShipmentCommandHandler : IRequestHandler<ConfirmShipmentCommand, ShipmentResponse>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IShipmentEventPublisher _eventPublisher;

    public ConfirmShipmentCommandHandler(
        IShipmentRepository shipmentRepository,
        IShipmentEventPublisher eventPublisher)
    {
        _shipmentRepository = shipmentRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<ShipmentResponse> Handle(ConfirmShipmentCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
        if (aggregate == null)
            throw new InvalidOperationException("Shipment not found");

        if (aggregate.Shipment.CustomerId != request.CustomerId)
            throw new UnauthorizedAccessException("Not authorized to confirm this shipment");

        aggregate.ConfirmBooking();
        await _shipmentRepository.UpdateAsync(aggregate.Shipment);

        // Publish domain events to RabbitMQ
        foreach (var domainEvent in aggregate.DomainEvents)
        {
            if (domainEvent is ShipmentBooked shipmentBooked)
            {
                await _eventPublisher.PublishAsync(shipmentBooked);
            }
        }

        aggregate.ClearDomainEvents();

        return new ShipmentResponse
        {
            Id = aggregate.Shipment.Id,
            TrackingNumber = aggregate.Shipment.TrackingNumber,
            CustomerId = aggregate.Shipment.CustomerId,
            Status = aggregate.Shipment.Status,
            TotalCost = aggregate.Shipment.TotalCost,
            Currency = aggregate.Shipment.Currency,
            EstimatedDeliveryDate = aggregate.Shipment.EstimatedDeliveryDate,
            ActualDeliveryDate = aggregate.Shipment.ActualDeliveryDate,
            CreatedAt = aggregate.Shipment.CreatedAt
        };
    }
}
