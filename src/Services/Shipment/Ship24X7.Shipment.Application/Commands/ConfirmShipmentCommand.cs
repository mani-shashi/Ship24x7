using MediatR;
using Ship24X7.Shipment.Application.DTOs;

namespace Ship24X7.Shipment.Application.Commands;

/// <summary>
/// Command for confirmshipment operation. Encapsulates request data and validation rules.
/// </summary>
public class ConfirmShipmentCommand : IRequest<ShipmentResponse>
{
    /// <summary>
    /// Gets or sets the shipmentId.
    /// </summary>
    public Guid ShipmentId { get; set; }
    /// <summary>
    /// Gets or sets the customerId.
    /// </summary>
    public Guid CustomerId { get; set; }
}
