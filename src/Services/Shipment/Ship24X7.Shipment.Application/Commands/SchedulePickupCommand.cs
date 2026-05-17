using MediatR;
using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Application.Commands;

/// <summary>
/// Command for schedulepickup operation. Encapsulates request data and validation rules.
/// </summary>
public class SchedulePickupCommand : IRequest<Guid>
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
    /// Gets or sets the pickupdate.
    /// </summary>
    public DateTime PickupDate { get; set; }
    /// <summary>
    /// Gets or sets the timeslot.
    /// </summary>
    public PickupTimeSlot TimeSlot { get; set; }
    /// <summary>
    /// Gets or sets the notes.
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}
