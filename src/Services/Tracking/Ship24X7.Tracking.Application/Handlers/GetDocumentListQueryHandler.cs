using MediatR;
using Ship24X7.Tracking.Application.DTOs;
using Ship24X7.Tracking.Application.Interfaces;
using Ship24X7.Tracking.Application.Queries;

namespace Ship24X7.Tracking.Application.Handlers;

/// <summary>
/// Query for retrieving getdocumentlisthandler data. Defines query parameters and result type.
/// </summary>
public class GetDocumentListQueryHandler : IRequestHandler<GetDocumentListQuery, List<DocumentResponse>>
{
    private readonly IDocumentRepository _documentRepository;

    public GetDocumentListQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<List<DocumentResponse>> Handle(GetDocumentListQuery request, CancellationToken cancellationToken)
    {
        var documents = await _documentRepository.GetByShipmentIdAsync(request.ShipmentId, cancellationToken);
        
        return documents.Select(d => new DocumentResponse
        {
            Id = d.Id,
            ShipmentId = d.ShipmentId,
            TrackingNumber = d.TrackingNumber,
            DocumentType = d.DocumentType,
            FileName = d.FileName,
            FileUrl = d.FileUrl,
            ContentType = d.ContentType,
            FileSizeBytes = d.FileSizeBytes,
            UploadedAt = d.UploadedAt
        }).ToList();
    }
}
