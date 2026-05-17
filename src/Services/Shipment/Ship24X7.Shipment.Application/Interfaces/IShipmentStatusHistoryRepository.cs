using Ship24X7.Shipment.Domain.Entities;

namespace Ship24X7.Shipment.Application.Interfaces;

/// <summary>
/// Append-only repository for shipment status history.
/// Rows are never updated or deleted.
/// </summary>
public interface IShipmentStatusHistoryRepository
{
    /// <summary>Returns all history rows for a shipment, ordered oldest-first.</summary>
    Task<List<ShipmentStatusHistory>> GetByShipmentIdAsync(Guid shipmentId);

    /// <summary>Persists a batch of new history rows (called after aggregate operations).</summary>
    Task AddRangeAsync(IEnumerable<ShipmentStatusHistory> entries);
}
