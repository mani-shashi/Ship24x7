using Microsoft.EntityFrameworkCore;
using Ship24X7.Tracking.Application.Interfaces;
using Ship24X7.Tracking.Domain.Entities;
using Ship24X7.Tracking.Infrastructure.Persistence;

namespace Ship24X7.Tracking.Infrastructure.Repositories;

/// <summary>
/// Repository for managing tracking event persistence and retrieval operations.
/// Handles database CRUD operations for shipment status updates, location changes, and exception events.
/// </summary>
public class TrackingEventRepository : ITrackingEventRepository
{
    private readonly TrackingDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackingEventRepository"/> class.
    /// </summary>
    /// <param name="context">Database context for tracking event operations.</param>
    public TrackingEventRepository(TrackingDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Adds a new tracking event to the database.
    /// Records shipment status changes, location updates, and exception events with timestamps.
    /// </summary>
    /// <param name="trackingEvent">The tracking event entity to add.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The added tracking event entity with generated ID.</returns>
    public async Task<TrackingEvent> AddAsync(TrackingEvent trackingEvent, CancellationToken cancellationToken = default)
    {
        await _context.TrackingEvents.AddAsync(trackingEvent, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return trackingEvent;
    }

    /// <summary>
    /// Retrieves all tracking events for a specific tracking number.
    /// Returns events ordered by timestamp (newest first) for chronological tracking history display.
    /// Used for public tracking pages and customer notifications.
    /// </summary>
    /// <param name="trackingNumber">The tracking number of the shipment.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of tracking events ordered by event timestamp descending.</returns>
    public async Task<List<TrackingEvent>> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default)
    {
        return await _context.TrackingEvents
            .Where(e => e.TrackingNumber == trackingNumber)
            .OrderByDescending(e => e.EventTimestamp)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves all tracking events for a specific shipment ID.
    /// Returns events ordered by timestamp (newest first) for internal shipment management.
    /// </summary>
    /// <param name="shipmentId">The unique identifier of the shipment.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of tracking events ordered by event timestamp descending.</returns>
    public async Task<List<TrackingEvent>> GetByShipmentIdAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        return await _context.TrackingEvents
            .Where(e => e.ShipmentId == shipmentId)
            .OrderByDescending(e => e.EventTimestamp)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves the most recent tracking event for a shipment.
    /// Used to determine current shipment status and validate state transitions.
    /// </summary>
    /// <param name="shipmentId">The unique identifier of the shipment.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The latest tracking event if found; otherwise, null.</returns>
    public async Task<TrackingEvent?> GetLatestByShipmentIdAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        return await _context.TrackingEvents
            .Where(e => e.ShipmentId == shipmentId)
            .OrderByDescending(e => e.EventTimestamp)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
