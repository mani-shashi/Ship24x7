using MediatR;

namespace Ship24X7.Tracking.Application.Queries;

/// <summary>
/// Query for retrieving getdocumentdownloadurl data. Defines query parameters and result type.
/// </summary>
public class GetDocumentDownloadUrlQuery : IRequest<string>
{
    /// <summary>
    /// Gets or sets the documentid.
    /// </summary>
    public Guid DocumentId { get; set; }
    /// <summary>
    /// Gets or sets the customerId.
    /// </summary>
    public Guid? CustomerId { get; set; } // For authorization
}
