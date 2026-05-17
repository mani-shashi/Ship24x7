using Ship24X7.Shipment.Domain.Entities;
using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Application.Interfaces;

/// <summary>
/// Repository for managing IPickup persistence operations. Handles database CRUD operations and queries.
/// </summary>
public interface IPickupRepository
{
    Task<Pickup?> GetByIdAsync(Guid id);
    Task<Pickup?> GetByShipmentIdAsync(Guid shipmentId);
    Task<Pickup> AddAsync(Pickup pickup);
    Task UpdateAsync(Pickup pickup);
    Task<int> GetPendingPickupCountAsync();
}
