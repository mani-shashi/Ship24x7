using MediatR;
using Ship24X7.Tracking.Application.Interfaces;
using Ship24X7.Tracking.Application.Queries;

namespace Ship24X7.Tracking.Application.Handlers;

/// <summary>
/// Query for retrieving getdocumentdownloadurlhandler data. Defines query parameters and result type.
/// </summary>
public class GetDocumentDownloadUrlQueryHandler : IRequestHandler<GetDocumentDownloadUrlQuery, string>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentStorageService _documentStorageService;

    public GetDocumentDownloadUrlQueryHandler(
        IDocumentRepository documentRepository,
        IDocumentStorageService documentStorageService)
    {
        _documentRepository = documentRepository;
        _documentStorageService = documentStorageService;
    }

    public async Task<string> Handle(GetDocumentDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);
        
        if (document == null)
        {
            throw new InvalidOperationException($"Document with ID {request.DocumentId} not found");
        }

        // Generate time-limited download URL (60 minutes)
        var downloadUrl = await _documentStorageService.GenerateDownloadUrlAsync(
            document.FileUrl, 
            60, 
            cancellationToken);

        return downloadUrl;
    }
}
