using MediatR;
using Ship24X7.Shipment.Application.DTOs;

namespace Ship24X7.Shipment.Application.Queries;

/// <summary>
/// Query for retrieving getshipmentbytrackingnumber data. Defines query parameters and result type.
/// </summary>
public class GetShipmentByTrackingNumberQuery : IRequest<ShipmentResponse?>
{
    /// <summary>
    /// Gets or sets the trackingNumber.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the customerId.
    /// </summary>
    public Guid? CustomerId { get; set; } // For authorization
}
