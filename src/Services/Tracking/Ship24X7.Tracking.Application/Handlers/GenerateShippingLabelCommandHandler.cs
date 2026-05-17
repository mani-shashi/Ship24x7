using MediatR;
using Ship24X7.Tracking.Application.Commands;
using Ship24X7.Tracking.Application.DTOs;
using Ship24X7.Tracking.Application.Interfaces;
using Ship24X7.Tracking.Domain.Entities;
using Ship24X7.Tracking.Domain.Enums;

namespace Ship24X7.Tracking.Application.Handlers;

/// <summary>
/// Handler for processing GenerateShippingLabel requests. Implements business logic and coordinates with repositories and services.
/// </summary>
public class GenerateShippingLabelCommandHandler : IRequestHandler<GenerateShippingLabelCommand, DocumentResponse>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentStorageService _documentStorageService;
    private readonly ILabelGenerationService _labelGenerationService;

    public GenerateShippingLabelCommandHandler(
        IDocumentRepository documentRepository,
        IDocumentStorageService documentStorageService,
        ILabelGenerationService labelGenerationService)
    {
        _documentRepository = documentRepository;
        _documentStorageService = documentStorageService;
        _labelGenerationService = labelGenerationService;
    }

    public async Task<DocumentResponse> Handle(GenerateShippingLabelCommand request, CancellationToken cancellationToken)
    {
        // Generate label PDF
        var labelPdf = await _labelGenerationService.GenerateShippingLabelAsync(
            request.TrackingNumber,
            request.SenderName,
            request.SenderAddress,
            request.ReceiverName,
            request.ReceiverAddress,
            request.ServiceType,
            request.Weight,
            cancellationToken);

        // Upload label
        var fileName = $"{request.TrackingNumber}_label.pdf";
        var fileUrl = await _documentStorageService.UploadDocumentAsync(
            fileName,
            labelPdf,
            "application/pdf",
            cancellationToken);

        // Create document record
        var document = new Document
        {
            Id = Guid.NewGuid(),
            ShipmentId = request.ShipmentId,
            TrackingNumber = request.TrackingNumber,
            DocumentType = DocumentType.ShippingLabel,
            FileName = fileName,
            FileUrl = fileUrl,
            ContentType = "application/pdf",
            FileSizeBytes = labelPdf.Length,
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
