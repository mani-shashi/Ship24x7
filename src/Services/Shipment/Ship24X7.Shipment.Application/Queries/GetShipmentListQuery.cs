using MediatR;
using Ship24X7.Shipment.Application.DTOs;
using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Application.Queries;

/// <summary>
/// Query for retrieving getshipmentlist data. Defines query parameters and result type.
/// </summary>
public class GetShipmentListQuery : IRequest<List<ShipmentResponse>>
{
    /// <summary>
    /// Gets or sets the customerId.
    /// </summary>
    public Guid? CustomerId { get; set; }
    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public ShipmentStatus? Status { get; set; }
    /// <summary>
    /// Gets or sets the fromdate.
    /// </summary>
    public DateTime? FromDate { get; set; }
    /// <summary>
    /// Gets or sets the todate.
    /// </summary>
    public DateTime? ToDate { get; set; }
    /// <summary>
    /// Gets or sets the skip.
    /// </summary>
    public int Skip { get; set; } = 0;
    /// <summary>
    /// Gets or sets the take.
    /// </summary>
    public int Take { get; set; } = 100;
}
