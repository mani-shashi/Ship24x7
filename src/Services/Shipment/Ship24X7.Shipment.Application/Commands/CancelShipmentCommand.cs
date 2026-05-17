using MediatR;

namespace Ship24X7.Shipment.Application.Commands;

/// <summary>
/// Command for cancelshipment operation. Encapsulates request data and validation rules.
/// </summary>
public class CancelShipmentCommand : IRequest<bool>
{
    /// <summary>
    /// Gets or sets the shipmentId.
    /// </summary>
    public Guid ShipmentId { get; set; }
    /// <summary>
    /// Gets or sets the customerId.
    /// </summary>
    public Guid CustomerId { get; set; }
    /// <summary>
    /// Gets or sets the reason.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
