using MediatR;
using Ship24X7.Payment.Application.DTOs;

namespace Ship24X7.Payment.Application.Queries;

/// <summary>
/// Query for retrieving getrefundstatus data. Defines query parameters and result type.
/// </summary>
public class GetRefundStatusQuery : IRequest<RefundDto?>
{
    /// <summary>
    /// Gets or sets the refundId.
    /// </summary>
    public Guid RefundId { get; set; }
}
