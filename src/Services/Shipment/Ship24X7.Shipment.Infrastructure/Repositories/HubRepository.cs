using Microsoft.EntityFrameworkCore;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Domain.Entities;
using Ship24X7.Shipment.Infrastructure.Persistence;

namespace Ship24X7.Shipment.Infrastructure.Repositories;

/// <summary>
/// Repository for managing hub persistence and retrieval operations.
/// Handles database CRUD operations for logistics hubs and distribution centers.
/// Supports hub capacity management and active hub filtering for routing decisions.
/// </summary>
public class HubRepository : IHubRepository
{
    private readonly ShipmentDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="HubRepository"/> class.
    /// </summary>
    /// <param name="context">Database context for hub operations.</param>
    public HubRepository(ShipmentDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a hub by its unique identifier.
    /// Used for hub details lookup and shipment assignment validation.
    /// </summary>
    /// <param name="id">The unique identifier of the hub.</param>
    /// <returns>The hub entity if found; otherwise, null.</returns>
    public async Task<Hub?> GetByIdAsync(Guid id)
    {
        return await _context.Hubs.FindAsync(id);
    }

    /// <summary>
    /// Retrieves all hubs with optional active-only filtering.
    /// Used for hub selection in shipment routing and nearest hub assignment.
    /// </summary>
    /// <param name="activeOnly">If true, returns only active hubs; if false, returns all hubs including inactive (default true).</param>
    /// <returns>List of hub entities matching the filter criteria.</returns>
    public async Task<List<Hub>> GetAllAsync(bool activeOnly = true)
    {
        var query = _context.Hubs.AsQueryable();

        if (activeOnly)
            query = query.Where(h => h.IsActive);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Adds a new hub to the database.
    /// Creates hub record with location, capacity, and operational details.
    /// Used for network expansion and new distribution center setup.
    /// </summary>
    /// <param name="hub">The hub entity to add.</param>
    /// <returns>The added hub entity with generated ID.</returns>
    public async Task<Hub> AddAsync(Hub hub)
    {
        await _context.Hubs.AddAsync(hub);
        await _context.SaveChangesAsync();
        return hub;
    }

    /// <summary>
    /// Updates an existing hub in the database.
    /// Handles capacity updates, load tracking, status changes, and location corrections.
    /// </summary>
    /// <param name="hub">The hub entity with updated values.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    public async Task UpdateAsync(Hub hub)
    {
        _context.Hubs.Update(hub);
        await _context.SaveChangesAsync();
    }
}
