using Ship24X7.Tracking.Domain.Entities;

namespace Ship24X7.Tracking.Application.Interfaces;

/// <summary>
/// Repository for managing IDocument persistence operations. Handles database CRUD operations and queries.
/// </summary>
public interface IDocumentRepository
{
    Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default);
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Document>> GetByShipmentIdAsync(Guid shipmentId, CancellationToken cancellationToken = default);
    Task<List<Document>> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default);
}
