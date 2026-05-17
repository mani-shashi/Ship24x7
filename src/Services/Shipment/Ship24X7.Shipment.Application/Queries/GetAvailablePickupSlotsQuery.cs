using MediatR;
using Ship24X7.Shipment.Application.DTOs;

namespace Ship24X7.Shipment.Application.Queries;

/// <summary>
/// Query for retrieving getavailablepickupslots data. Defines query parameters and result type.
/// </summary>
public class GetAvailablePickupSlotsQuery : IRequest<List<PickupSlotResponse>>
{
    /// <summary>
    /// Gets or sets the date.
    /// </summary>
    public DateTime Date { get; set; }
}
