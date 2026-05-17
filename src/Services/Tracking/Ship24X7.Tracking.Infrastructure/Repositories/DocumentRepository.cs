using Microsoft.EntityFrameworkCore;
using Ship24X7.Tracking.Application.Interfaces;
using Ship24X7.Tracking.Domain.Entities;
using Ship24X7.Tracking.Infrastructure.Persistence;

namespace Ship24X7.Tracking.Infrastructure.Repositories;

/// <summary>
/// Repository for managing shipment document persistence and retrieval operations.
/// Handles database CRUD operations for shipping labels, customs declarations, invoices, and other shipment documents.
/// </summary>
public class DocumentRepository : IDocumentRepository
{
    private readonly TrackingDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentRepository"/> class.
    /// </summary>
    /// <param name="context">Database context for document operations.</param>
    public DocumentRepository(TrackingDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Adds a new document record to the database.
    /// Stores document metadata including file URL, type, size, and upload information.
    /// </summary>
    /// <param name="document">The document entity to add.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The added document entity with generated ID.</returns>
    public async Task<Document> AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await _context.Documents.AddAsync(document, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return document;
    }

    /// <summary>
    /// Retrieves a document by its unique identifier.
    /// Used for accessing specific document details and generating download URLs.
    /// </summary>
    /// <param name="id">The unique identifier of the document.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The document entity if found; otherwise, null.</returns>
    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves all documents associated with a shipment.
    /// Returns documents ordered by upload date (newest first) for chronological display.
    /// </summary>
    /// <param name="shipmentId">The unique identifier of the shipment.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of document entities ordered by upload date descending.</returns>
    public async Task<List<Document>> GetByShipmentIdAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Where(d => d.ShipmentId == shipmentId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves all documents associated with a tracking number.
    /// Enables public access to shipment documents using tracking number.
    /// Returns documents ordered by upload date (newest first).
    /// </summary>
    /// <param name="trackingNumber">The tracking number of the shipment.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A list of document entities ordered by upload date descending.</returns>
    public async Task<List<Document>> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Where(d => d.TrackingNumber == trackingNumber)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(cancellationToken);
    }
}
