using MediatR;
using Ship24X7.Shipment.Application.DTOs;

namespace Ship24X7.Shipment.Application.Commands;

/// <summary>
/// Command for updateshipment operation. Encapsulates request data and validation rules.
/// </summary>
public class UpdateShipmentCommand : IRequest<ShipmentResponse>
{
    /// <summary>
    /// Gets or sets the shipmentId.
    /// </summary>
    public Guid ShipmentId { get; set; }
    /// <summary>
    /// Gets or sets the customerId.
    /// </summary>
    public Guid CustomerId { get; set; }
    
    // Optional fields that can be updated while in Draft status
    /// <summary>
    /// Gets or sets the declaredvalue.
    /// </summary>
    public decimal? DeclaredValue { get; set; }
    /// <summary>
    /// Gets or sets the isfragile.
    /// </summary>
    public bool? IsFragile { get; set; }
    /// <summary>
    /// Gets or sets the requiresrefrigeration.
    /// </summary>
    public bool? RequiresRefrigeration { get; set; }
    public List<ShipmentItemDto>? Items { get; set; }
}
