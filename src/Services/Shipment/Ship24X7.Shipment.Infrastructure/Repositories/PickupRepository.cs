using Microsoft.EntityFrameworkCore;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Domain.Entities;
using Ship24X7.Shipment.Domain.Enums;
using Ship24X7.Shipment.Infrastructure.Persistence;

namespace Ship24X7.Shipment.Infrastructure.Repositories;

/// <summary>
/// Repository for managing pickup persistence and retrieval operations.
/// Handles database CRUD operations for pickup scheduling, driver assignment, and completion tracking.
/// </summary>
public class PickupRepository : IPickupRepository
{
    private readonly ShipmentDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PickupRepository"/> class.
    /// </summary>
    /// <param name="context">Database context for pickup operations.</param>
    public PickupRepository(ShipmentDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a pickup by its unique identifier with shipment details.
    /// Includes related shipment for complete pickup context.
    /// </summary>
    /// <param name="id">The unique identifier of the pickup.</param>
    /// <returns>The pickup entity if found; otherwise, null.</returns>
    public async Task<Pickup?> GetByIdAsync(Guid id)
    {
        return await _context.Pickups
            .Include(p => p.Shipment)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Retrieves a pickup by shipment ID.
    /// Used to check if pickup is already scheduled for a shipment.
    /// </summary>
    /// <param name="shipmentId">The unique identifier of the shipment.</param>
    /// <returns>The pickup entity if found; otherwise, null.</returns>
    public async Task<Pickup?> GetByShipmentIdAsync(Guid shipmentId)
    {
        return await _context.Pickups
            .FirstOrDefaultAsync(p => p.ShipmentId == shipmentId);
    }

    /// <summary>
    /// Adds a new pickup schedule to the database.
    /// Creates pickup record with confirmation number, date, time slot, and initial status.
    /// </summary>
    /// <param name="pickup">The pickup entity to add.</param>
    /// <returns>The added pickup entity with generated ID.</returns>
    public async Task<Pickup> AddAsync(Pickup pickup)
    {
        await _context.Pickups.AddAsync(pickup);
        await _context.SaveChangesAsync();
        return pickup;
    }

    /// <summary>
    /// Updates an existing pickup in the database.
    /// Handles driver assignment, status changes, and completion updates.
    /// </summary>
    /// <param name="pickup">The pickup entity with updated values.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    public async Task UpdateAsync(Pickup pickup)
    {
        _context.Pickups.Update(pickup);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets the count of pending pickups requiring driver assignment or completion.
    /// Includes pickups in Scheduled and EnRoute status for operational monitoring.
    /// Used for dashboard statistics and driver workload management.
    /// </summary>
    /// <returns>Number of pickups in Scheduled or EnRoute status.</returns>
    public async Task<int> GetPendingPickupCountAsync()
    {
        return await _context.Pickups
            .Where(p => p.Status == PickupStatus.Scheduled || p.Status == PickupStatus.EnRoute)
            .CountAsync();
    }
}
