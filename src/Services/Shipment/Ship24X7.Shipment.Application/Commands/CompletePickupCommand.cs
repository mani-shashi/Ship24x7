using MediatR;

namespace Ship24X7.Shipment.Application.Commands;

/// <summary>
/// Command for completepickup operation. Encapsulates request data and validation rules.
/// </summary>
public class CompletePickupCommand : IRequest<bool>
{
    /// <summary>
    /// Gets or sets the pickupid.
    /// </summary>
    public Guid PickupId { get; set; }
    /// <summary>
    /// Gets or sets the driverid.
    /// </summary>
    public Guid? DriverId { get; set; }
}
