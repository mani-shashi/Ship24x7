using MediatR;
using Ship24X7.Payment.Application.DTOs;

namespace Ship24X7.Payment.Application.Queries;

/// <summary>
/// Query for retrieving getpaymentorderbyshipment data. Defines query parameters and result type.
/// </summary>
public class GetPaymentOrderByShipmentQuery : IRequest<PaymentOrderDto?>
{
    /// <summary>
    /// Gets or sets the shipmentId.
    /// </summary>
    public Guid ShipmentId { get; set; }
}
