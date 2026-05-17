using Ship24X7.Shipment.Domain.Aggregates;
using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Application.Interfaces;

/// <summary>
/// Repository for managing IShipment persistence operations. Handles database CRUD operations and queries.
/// </summary>
public interface IShipmentRepository
{
    Task<ShipmentAggregate?> GetByIdAsync(Guid id);
    Task<ShipmentAggregate?> GetByTrackingNumberAsync(string trackingNumber);
    Task<ShipmentAggregate?> GetByIdempotencyKeyAsync(string idempotencyKey);
    Task<Domain.Entities.Shipment?> GetByIdWithDetailsAsync(Guid id);
    Task<Domain.Entities.Shipment?> GetByTrackingNumberWithDetailsAsync(string trackingNumber);
    Task<List<Domain.Entities.Shipment>> GetListAsync(
        Guid? customerId = null,
        ShipmentStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int skip = 0,
        int take = 100);
    Task<Domain.Entities.Shipment> AddAsync(Domain.Entities.Shipment shipment);
    Task UpdateAsync(Domain.Entities.Shipment shipment);
    Task<int> GetTodayBookingCountAsync();
    Task<int> GetCountByStatusAsync(ShipmentStatus status);
    Task<int> GetDeliveredTodayCountAsync();
}
