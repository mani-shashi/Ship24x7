using Microsoft.EntityFrameworkCore;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Domain.Aggregates;
using Ship24X7.Shipment.Domain.Enums;
using Ship24X7.Shipment.Infrastructure.Persistence;

namespace Ship24X7.Shipment.Infrastructure.Repositories;

/// <summary>
/// Repository for managing shipment persistence and retrieval operations.
/// Handles database CRUD operations, queries with filters, and aggregate statistics.
/// Returns ShipmentAggregate for domain-driven design with encapsulated business logic.
/// </summary>
public class ShipmentRepository : IShipmentRepository
{
    private readonly ShipmentDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShipmentRepository"/> class.
    /// </summary>
    /// <param name="context">Database context for shipment operations.</param>
    public ShipmentRepository(ShipmentDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a shipment aggregate by its unique identifier with all related entities.
    /// Includes items, pickup, sender/receiver addresses, and service rate for complete shipment context.
    /// </summary>
    /// <param name="id">The unique identifier of the shipment.</param>
    /// <returns>Shipment aggregate if found; otherwise, null.</returns>
    public async Task<ShipmentAggregate?> GetByIdAsync(Guid id)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Items)
            .Include(s => s.Pickup)
            .Include(s => s.SenderAddress)
            .Include(s => s.ReceiverAddress)
            .Include(s => s.ServiceRate)
            .Include(s => s.StatusHistory)
            .FirstOrDefaultAsync(s => s.Id == id);

        return shipment == null ? null : new ShipmentAggregate(shipment);
    }

    /// <summary>
    /// Retrieves a shipment aggregate by tracking number with all related entities.
    /// Enables public shipment lookup without exposing internal shipment ID.
    /// </summary>
    /// <param name="trackingNumber">The tracking number of the shipment.</param>
    /// <returns>Shipment aggregate if found; otherwise, null.</returns>
    public async Task<ShipmentAggregate?> GetByTrackingNumberAsync(string trackingNumber)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Items)
            .Include(s => s.Pickup)
            .Include(s => s.SenderAddress)
            .Include(s => s.ReceiverAddress)
            .Include(s => s.ServiceRate)
            .Include(s => s.StatusHistory)
            .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);

        return shipment == null ? null : new ShipmentAggregate(shipment);
    }

    /// <summary>
    /// Retrieves a shipment aggregate by idempotency key for duplicate prevention.
    /// Prevents creating duplicate shipments from retried API requests.
    /// </summary>
    /// <param name="idempotencyKey">The idempotency key from the original request.</param>
    /// <returns>Shipment aggregate if found; otherwise, null.</returns>
    public async Task<ShipmentAggregate?> GetByIdempotencyKeyAsync(string idempotencyKey)
    {
        var shipment = await _context.Shipments
            .Include(s => s.Items)
            .Include(s => s.Pickup)
            .Include(s => s.SenderAddress)
            .Include(s => s.ReceiverAddress)
            .Include(s => s.ServiceRate)
            .Include(s => s.StatusHistory)
            .FirstOrDefaultAsync(s => s.IdempotencyKey == idempotencyKey);

        return shipment == null ? null : new ShipmentAggregate(shipment);
    }

    /// <summary>
    /// Retrieves a shipment by its unique identifier with all related entities for detailed view.
    /// Includes items, sender/receiver addresses, and service rate for complete shipment details.
    /// </summary>
    /// <param name="id">The unique identifier of the shipment.</param>
    /// <returns>Shipment entity if found; otherwise, null.</returns>
    public async Task<Domain.Entities.Shipment?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Shipments
            .Include(s => s.Items)
            .Include(s => s.SenderAddress)
            .Include(s => s.ReceiverAddress)
            .Include(s => s.ServiceRate)
            .Include(s => s.Pickup)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    /// <summary>
    /// Retrieves a shipment by tracking number with all related entities for detailed view.
    /// Includes items, sender/receiver addresses, and service rate for complete shipment details.
    /// </summary>
    /// <param name="trackingNumber">The tracking number of the shipment.</param>
    /// <returns>Shipment entity if found; otherwise, null.</returns>
    public async Task<Domain.Entities.Shipment?> GetByTrackingNumberWithDetailsAsync(string trackingNumber)
    {
        return await _context.Shipments
            .Include(s => s.Items)
            .Include(s => s.SenderAddress)
            .Include(s => s.ReceiverAddress)
            .Include(s => s.ServiceRate)
            .Include(s => s.Pickup)
            .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);
    }

    /// <summary>
    /// Retrieves a paginated list of shipments with optional filters.
    /// Supports filtering by customer, status, and date range for dashboard and reporting.
    /// Results are ordered by creation date (newest first) with pagination.
    /// </summary>
    /// <param name="customerId">Optional customer ID filter to show only customer's shipments.</param>
    /// <param name="status">Optional status filter to show shipments in specific state.</param>
    /// <param name="fromDate">Optional start date filter for date range queries.</param>
    /// <param name="toDate">Optional end date filter for date range queries.</param>
    /// <param name="skip">Number of records to skip for pagination (default 0).</param>
    /// <param name="take">Number of records to return per page (default 100, max 100).</param>
    /// <returns>List of shipment entities matching the filter criteria.</returns>
    public async Task<List<Domain.Entities.Shipment>> GetListAsync(
        Guid? customerId = null,
        ShipmentStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int skip = 0,
        int take = 100)
    {
        var query = _context.Shipments
            .Include(s => s.Items)
            .Include(s => s.SenderAddress)
            .Include(s => s.ReceiverAddress)
            .Include(s => s.ServiceRate)
            .Include(s => s.Pickup)
            .AsQueryable();

        if (customerId.HasValue)
            query = query.Where(s => s.CustomerId == customerId.Value);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(s => s.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(s => s.CreatedAt <= toDate.Value);

        return await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    /// <summary>
    /// Adds a new shipment to the database with all related entities.
    /// Persists shipment, items, and addresses in a single transaction.
    /// </summary>
    /// <param name="shipment">The shipment entity to add.</param>
    /// <returns>The added shipment entity with generated ID.</returns>
    public async Task<Domain.Entities.Shipment> AddAsync(Domain.Entities.Shipment shipment)
    {
        await _context.Shipments.AddAsync(shipment);
        await _context.SaveChangesAsync();
        return shipment;
    }

    /// <summary>
    /// Updates an existing shipment in the database.
    /// Handles status changes, pickup assignments, and delivery updates.
    /// </summary>
    /// <param name="shipment">The shipment entity with updated values.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    public async Task UpdateAsync(Domain.Entities.Shipment shipment)
    {
        _context.Shipments.Update(shipment);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets the count of shipments booked today.
    /// Used for dashboard statistics and daily booking metrics.
    /// </summary>
    /// <returns>Number of shipments with Booked status created today.</returns>
    public async Task<int> GetTodayBookingCountAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Shipments
            .Where(s => s.CreatedAt >= today && s.Status == ShipmentStatus.Booked)
            .CountAsync();
    }

    /// <summary>
    /// Gets the count of shipments in a specific status.
    /// Used for dashboard statistics and operational monitoring.
    /// </summary>
    /// <param name="status">The shipment status to count.</param>
    /// <returns>Number of shipments in the specified status.</returns>
    public async Task<int> GetCountByStatusAsync(ShipmentStatus status)
    {
        return await _context.Shipments
            .Where(s => s.Status == status)
            .CountAsync();
    }

    /// <summary>
    /// Gets the count of shipments delivered today.
    /// Used for dashboard statistics and daily delivery performance metrics.
    /// </summary>
    /// <returns>Number of shipments with Delivered status and ActualDeliveryDate today.</returns>
    public async Task<int> GetDeliveredTodayCountAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Shipments
            .Where(s => s.ActualDeliveryDate.HasValue && 
                       s.ActualDeliveryDate.Value >= today && 
                       s.Status == ShipmentStatus.Delivered)
            .CountAsync();
    }
}
