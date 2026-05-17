using MediatR;
using Ship24X7.Tracking.Application.Commands;
using Ship24X7.Tracking.Application.DTOs;
using Ship24X7.Tracking.Application.Interfaces;
using Ship24X7.Tracking.Domain.Entities;
using Ship24X7.Tracking.Domain.Enums;

namespace Ship24X7.Tracking.Application.Handlers;

/// <summary>
/// Handler for processing GenerateCustomsDeclaration requests. Implements business logic and coordinates with repositories and services.
/// </summary>
public class GenerateCustomsDeclarationCommandHandler : IRequestHandler<GenerateCustomsDeclarationCommand, DocumentResponse>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentStorageService _documentStorageService;
    private readonly ICustomsFormService _customsFormService;

    public GenerateCustomsDeclarationCommandHandler(
        IDocumentRepository documentRepository,
        IDocumentStorageService documentStorageService,
        ICustomsFormService customsFormService)
    {
        _documentRepository = documentRepository;
        _documentStorageService = documentStorageService;
        _customsFormService = customsFormService;
    }

    public async Task<DocumentResponse> Handle(GenerateCustomsDeclarationCommand request, CancellationToken cancellationToken)
    {
        // Generate customs declaration PDF
        var customsPdf = await _customsFormService.GenerateCustomsDeclarationAsync(
            request.TrackingNumber,
            request.SenderName,
            request.SenderCountry,
            request.ReceiverName,
            request.ReceiverCountry,
            request.DeclaredValue,
            request.Currency,
            request.Items,
            cancellationToken);

        // Upload customs declaration
        var fileName = $"{request.TrackingNumber}_customs.pdf";
        var fileUrl = await _documentStorageService.UploadDocumentAsync(
            fileName,
            customsPdf,
            "application/pdf",
            cancellationToken);

        // Create document record
        var document = new Document
        {
            Id = Guid.NewGuid(),
            ShipmentId = request.ShipmentId,
            TrackingNumber = request.TrackingNumber,
            DocumentType = DocumentType.CustomsDeclaration,
            FileName = fileName,
            FileUrl = fileUrl,
            ContentType = "application/pdf",
            FileSizeBytes = customsPdf.Length,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = Guid.Empty, // System generated
            CreatedAt = DateTime.UtcNow
        };

        await _documentRepository.AddAsync(document, cancellationToken);

        return new DocumentResponse
        {
            Id = document.Id,
            ShipmentId = document.ShipmentId,
            TrackingNumber = document.TrackingNumber,
            DocumentType = document.DocumentType,
            FileName = document.FileName,
            FileUrl = document.FileUrl,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            UploadedAt = document.UploadedAt
        };
    }
}
