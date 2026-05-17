using MediatR;
using Ship24X7.Tracking.Application.DTOs;

namespace Ship24X7.Tracking.Application.Queries;

/// <summary>
/// Query for retrieving getdocumentlist data. Defines query parameters and result type.
/// </summary>
public class GetDocumentListQuery : IRequest<List<DocumentResponse>>
{
    /// <summary>
    /// Gets or sets the shipmentId.
    /// </summary>
    public Guid ShipmentId { get; set; }
    /// <summary>
    /// Gets or sets the customerId.
    /// </summary>
    public Guid? CustomerId { get; set; } // For authorization
}
