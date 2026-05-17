using MediatR;
using Ship24X7.Shipment.Application.DTOs;

namespace Ship24X7.Shipment.Application.Queries;

/// <summary>
/// Returns the full status transition history for a shipment, ordered oldest-first.
/// </summary>
public class GetShipmentStatusHistoryQuery : IRequest<List<ShipmentStatusHistoryDto>>
{
    public Guid ShipmentId { get; set; }
}
