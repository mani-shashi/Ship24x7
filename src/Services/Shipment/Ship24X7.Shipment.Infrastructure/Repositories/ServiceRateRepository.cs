using Microsoft.EntityFrameworkCore;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Domain.Entities;
using Ship24X7.Shipment.Infrastructure.Persistence;

namespace Ship24X7.Shipment.Infrastructure.Repositories;

/// <summary>
/// Repository for managing service rate persistence and retrieval operations.
/// Handles database CRUD operations for shipping service rates and pricing configurations.
/// Supports active rate filtering and service type queries for rate calculation.
/// </summary>
public class ServiceRateRepository : IServiceRateRepository
{
    private readonly ShipmentDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceRateRepository"/> class.
    /// </summary>
    /// <param name="context">Database context for service rate operations.</param>
    public ServiceRateRepository(ShipmentDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a service rate by its unique identifier.
    /// Used for rate validation and shipment pricing lookup.
    /// </summary>
    /// <param name="id">The unique identifier of the service rate.</param>
    /// <returns>The service rate entity if found; otherwise, null.</returns>
    public async Task<ServiceRate?> GetByIdAsync(Guid id)
    {
        return await _context.ServiceRates.FindAsync(id);
    }

    /// <summary>
    /// Retrieves all active service rates with optional service type filtering.
    /// Used for rate calculation and displaying available shipping options to customers.
    /// Only returns rates marked as active to exclude discontinued or seasonal services.
    /// </summary>
    /// <param name="serviceType">Optional service type filter (e.g., "EXPRESS", "STANDARD", "ECONOMY"). If null, returns all active rates.</param>
    /// <returns>List of active service rate entities matching the filter criteria.</returns>
    public async Task<List<ServiceRate>> GetActiveRatesAsync(string? serviceType = null)
    {
        var query = _context.ServiceRates.Where(sr => sr.IsActive);

        if (!string.IsNullOrEmpty(serviceType))
            query = query.Where(sr => sr.ServiceType == serviceType);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Adds a new service rate to the database.
    /// Creates rate configuration with pricing structure, delivery time, and service details.
    /// Used for introducing new shipping services or pricing tiers.
    /// </summary>
    /// <param name="serviceRate">The service rate entity to add.</param>
    /// <returns>The added service rate entity with generated ID.</returns>
    public async Task<ServiceRate> AddAsync(ServiceRate serviceRate)
    {
        await _context.ServiceRates.AddAsync(serviceRate);
        await _context.SaveChangesAsync();
        return serviceRate;
    }

    /// <summary>
    /// Updates an existing service rate in the database.
    /// Handles pricing updates, surcharge adjustments, delivery time changes, and activation status.
    /// </summary>
    /// <param name="serviceRate">The service rate entity with updated values.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    public async Task UpdateAsync(ServiceRate serviceRate)
    {
        _context.ServiceRates.Update(serviceRate);
        await _context.SaveChangesAsync();
    }
}
