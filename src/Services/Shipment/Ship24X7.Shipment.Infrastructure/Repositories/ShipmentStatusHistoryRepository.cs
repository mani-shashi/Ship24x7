using Microsoft.EntityFrameworkCore;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Domain.Entities;
using Ship24X7.Shipment.Infrastructure.Persistence;

namespace Ship24X7.Shipment.Infrastructure.Repositories;

/// <summary>
/// Append-only EF Core repository for ShipmentStatusHistory.
/// No Update or Delete methods — history rows are immutable.
/// </summary>
public class ShipmentStatusHistoryRepository : IShipmentStatusHistoryRepository
{
    private readonly ShipmentDbContext _context;

    public ShipmentStatusHistoryRepository(ShipmentDbContext context)
    {
        _context = context;
    }

    public async Task<List<ShipmentStatusHistory>> GetByShipmentIdAsync(Guid shipmentId)
    {
        return await _context.ShipmentStatusHistory
            .Where(h => h.ShipmentId == shipmentId)
            .OrderBy(h => h.ChangedAt)
            .ToListAsync();
    }

    public async Task AddRangeAsync(IEnumerable<ShipmentStatusHistory> entries)
    {
        await _context.ShipmentStatusHistory.AddRangeAsync(entries);
        await _context.SaveChangesAsync();
    }
}
