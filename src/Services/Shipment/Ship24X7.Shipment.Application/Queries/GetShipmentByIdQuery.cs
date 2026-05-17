using MediatR;
using Ship24X7.Shipment.Application.DTOs;

namespace Ship24X7.Shipment.Application.Queries;

/// <summary>
/// Query for retrieving getshipmentbyid data. Defines query parameters and result type.
/// </summary>
public class GetShipmentByIdQuery : IRequest<ShipmentResponse?>
{
    /// <summary>
    /// Gets or sets the shipmentId.
    /// </summary>
    public Guid ShipmentId { get; set; }
    /// <summary>
    /// Gets or sets the customerId.
    /// </summary>
    public Guid? CustomerId { get; set; } // For authorization
}
