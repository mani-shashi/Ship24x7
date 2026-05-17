using Ship24X7.Tracking.Domain.Entities;

namespace Ship24X7.Tracking.Application.Interfaces;

/// <summary>
/// Repository for managing IDeliveryProof persistence operations. Handles database CRUD operations and queries.
/// </summary>
public interface IDeliveryProofRepository
{
    Task<DeliveryProof> AddAsync(DeliveryProof deliveryProof, CancellationToken cancellationToken = default);
    Task<DeliveryProof?> GetByShipmentIdAsync(Guid shipmentId, CancellationToken cancellationToken = default);
    Task<DeliveryProof?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default);
}
