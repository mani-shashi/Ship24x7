using Microsoft.EntityFrameworkCore;
using Ship24X7.Tracking.Application.Interfaces;
using Ship24X7.Tracking.Domain.Entities;
using Ship24X7.Tracking.Infrastructure.Persistence;

namespace Ship24X7.Tracking.Infrastructure.Repositories;

/// <summary>
/// Repository for managing delivery proof persistence and retrieval operations.
/// Handles database CRUD operations for delivery confirmation records including signatures, photos, and GPS coordinates.
/// </summary>
public class DeliveryProofRepository : IDeliveryProofRepository
{
    private readonly TrackingDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeliveryProofRepository"/> class.
    /// </summary>
    /// <param name="context">Database context for delivery proof operations.</param>
    public DeliveryProofRepository(TrackingDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Adds a new delivery proof record to the database.
    /// Stores signature image URL, photo proof URL, GPS coordinates, and recipient information.
    /// </summary>
    /// <param name="deliveryProof">The delivery proof entity to add.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The added delivery proof entity with generated ID.</returns>
    public async Task<DeliveryProof> AddAsync(DeliveryProof deliveryProof, CancellationToken cancellationToken = default)
    {
        await _context.DeliveryProofs.AddAsync(deliveryProof, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return deliveryProof;
    }

    /// <summary>
    /// Retrieves delivery proof by shipment ID.
    /// Used to verify delivery confirmation for a specific shipment.
    /// </summary>
    /// <param name="shipmentId">The unique identifier of the shipment.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The delivery proof entity if found; otherwise, null.</returns>
    public async Task<DeliveryProof?> GetByShipmentIdAsync(Guid shipmentId, CancellationToken cancellationToken = default)
    {
        return await _context.DeliveryProofs
            .FirstOrDefaultAsync(d => d.ShipmentId == shipmentId, cancellationToken);
    }

    /// <summary>
    /// Retrieves delivery proof by tracking number.
    /// Enables public access to delivery confirmation using tracking number.
    /// </summary>
    /// <param name="trackingNumber">The tracking number of the shipment.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The delivery proof entity if found; otherwise, null.</returns>
    public async Task<DeliveryProof?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default)
    {
        return await _context.DeliveryProofs
            .FirstOrDefaultAsync(d => d.TrackingNumber == trackingNumber, cancellationToken);
    }
}
