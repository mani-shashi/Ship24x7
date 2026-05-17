using Ship24X7.Tracking.Domain.Entities;

namespace Ship24X7.Tracking.Application.Interfaces;

/// <summary>
/// Repository for managing ITrackingEvent persistence operations. Handles database CRUD operations and queries.
/// </summary>
public interface ITrackingEventRepository
{
    Task<TrackingEvent> AddAsync(TrackingEvent trackingEvent, CancellationToken cancellationToken = default);
    Task<List<TrackingEvent>> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default);
    Task<List<TrackingEvent>> GetByShipmentIdAsync(Guid shipmentId, CancellationToken cancellationToken = default);
    Task<TrackingEvent?> GetLatestByShipmentIdAsync(Guid shipmentId, CancellationToken cancellationToken = default);
}
