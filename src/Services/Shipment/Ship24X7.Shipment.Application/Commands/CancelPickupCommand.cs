using MediatR;

namespace Ship24X7.Shipment.Application.Commands;

/// <summary>
/// Command for cancelpickup operation. Encapsulates request data and validation rules.
/// </summary>
public class CancelPickupCommand : IRequest<bool>
{
    /// <summary>
    /// Gets or sets the pickupid.
    /// </summary>
    public Guid PickupId { get; set; }
    /// <summary>
    /// Gets or sets the customerId.
    /// </summary>
    public Guid CustomerId { get; set; }
}
