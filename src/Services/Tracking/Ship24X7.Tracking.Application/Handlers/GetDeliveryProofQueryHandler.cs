using MediatR;
using Ship24X7.Tracking.Application.DTOs;
using Ship24X7.Tracking.Application.Interfaces;
using Ship24X7.Tracking.Application.Queries;

namespace Ship24X7.Tracking.Application.Handlers;

/// <summary>
/// Query for retrieving getdeliveryproofhandler data. Defines query parameters and result type.
/// </summary>
public class GetDeliveryProofQueryHandler : IRequestHandler<GetDeliveryProofQuery, DeliveryProofResponse?>
{
    private readonly IDeliveryProofRepository _deliveryProofRepository;

    public GetDeliveryProofQueryHandler(IDeliveryProofRepository deliveryProofRepository)
    {
        _deliveryProofRepository = deliveryProofRepository;
    }

    public async Task<DeliveryProofResponse?> Handle(GetDeliveryProofQuery request, CancellationToken cancellationToken)
    {
        var deliveryProof = await _deliveryProofRepository.GetByShipmentIdAsync(request.ShipmentId, cancellationToken);
        
        if (deliveryProof == null)
        {
            return null;
        }

        return new DeliveryProofResponse
        {
            Id = deliveryProof.Id,
            ShipmentId = deliveryProof.ShipmentId,
            TrackingNumber = deliveryProof.TrackingNumber,
            ReceivedBy = deliveryProof.ReceivedBy,
            DeliveryDate = deliveryProof.DeliveryDate,
            SignatureImageUrl = deliveryProof.SignatureImageUrl,
            PhotoProofUrl = deliveryProof.PhotoProofUrl,
            Latitude = deliveryProof.Latitude,
            Longitude = deliveryProof.Longitude,
            Notes = deliveryProof.Notes
        };
    }
}
