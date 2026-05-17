using MediatR;
using Ship24X7.Shipment.Application.Commands;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Domain.Entities;
using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Application.Handlers;

/// <summary>
/// Handler for processing SchedulePickup requests. Implements business logic and coordinates with repositories and services.
/// </summary>
public class SchedulePickupCommandHandler : IRequestHandler<SchedulePickupCommand, Guid>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IPickupRepository _pickupRepository;
    private readonly IPickupSlotValidator _pickupSlotValidator;

    public SchedulePickupCommandHandler(
        IShipmentRepository shipmentRepository,
        IPickupRepository pickupRepository,
        IPickupSlotValidator pickupSlotValidator)
    {
        _shipmentRepository = shipmentRepository;
        _pickupRepository = pickupRepository;
        _pickupSlotValidator = pickupSlotValidator;
    }

    public async Task<Guid> Handle(SchedulePickupCommand request, CancellationToken cancellationToken)
    {
        var aggregate = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
        if (aggregate == null)
            throw new InvalidOperationException("Shipment not found");

        if (aggregate.Shipment.CustomerId != request.CustomerId)
            throw new UnauthorizedAccessException("Not authorized to schedule pickup for this shipment");

        if (!_pickupSlotValidator.IsSlotAvailable(request.PickupDate, request.TimeSlot))
            throw new InvalidOperationException("Selected pickup slot is not available");

        var pickup = new Pickup
        {
            Id = Guid.NewGuid(),
            ShipmentId = request.ShipmentId,
            ConfirmationNumber = $"PU{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}",
            PickupDate = request.PickupDate,
            TimeSlot = request.TimeSlot,
            Status = PickupStatus.Scheduled,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CustomerId
        };

        aggregate.SchedulePickup(pickup);

        // Persist the pickup row first, then update the shipment
        await _pickupRepository.AddAsync(pickup);
        await _shipmentRepository.UpdateAsync(aggregate.Shipment);

        return pickup.Id;
    }
}
