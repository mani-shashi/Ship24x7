using MediatR;
using Ship24X7.Payment.Application.DTOs;

namespace Ship24X7.Payment.Application.Queries;

/// <summary>
/// Query for retrieving getpaymentorder data. Defines query parameters and result type.
/// </summary>
public class GetPaymentOrderQuery : IRequest<PaymentOrderDto?>
{
    /// <summary>
    /// Gets or sets the Payment orderId.
    /// </summary>
    public Guid PaymentOrderId { get; set; }
}
