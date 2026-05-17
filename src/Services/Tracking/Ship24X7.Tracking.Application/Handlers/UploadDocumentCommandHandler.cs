using MediatR;
using Ship24X7.Tracking.Application.Commands;
using Ship24X7.Tracking.Application.DTOs;
using Ship24X7.Tracking.Application.Interfaces;
using Ship24X7.Tracking.Domain.Entities;
using Ship24X7.Tracking.Domain.Events;

namespace Ship24X7.Tracking.Application.Handlers;

/// <summary>
/// Handler for processing UploadDocument requests. Implements business logic and coordinates with repositories and services.
/// </summary>
public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, DocumentResponse>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentStorageService _documentStorageService;
    private readonly IPublisher _publisher;

    public UploadDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IDocumentStorageService documentStorageService,
        IPublisher publisher)
    {
        _documentRepository = documentRepository;
        _documentStorageService = documentStorageService;
        _publisher = publisher;
    }

    public async Task<DocumentResponse> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        // Validate file size (max 10 MB)
        const long maxFileSizeBytes = 10 * 1024 * 1024;
        if (request.FileContent.Length > maxFileSizeBytes)
        {
            throw new InvalidOperationException("File size exceeds 10 MB limit");
        }

        // Validate file type
        var allowedContentTypes = new[] { "application/pdf", "image/jpeg", "image/jpg", "image/png" };
        if (!allowedContentTypes.Contains(request.ContentType.ToLower()))
        {
            throw new InvalidOperationException("File type must be PDF, JPG, or PNG");
        }

        // Upload document
        var fileUrl = await _documentStorageService.UploadDocumentAsync(
            request.FileName,
            request.FileContent,
            request.ContentType,
            cancellationToken);

        // Create document record
        var document = new Document
        {
            Id = Guid.NewGuid(),
            ShipmentId = request.ShipmentId,
            TrackingNumber = request.TrackingNumber,
            DocumentType = request.DocumentType,
            FileName = request.FileName,
            FileUrl = fileUrl,
            ContentType = request.ContentType,
            FileSizeBytes = request.FileContent.Length,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = request.UploadedBy,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.UploadedBy
        };

        await _documentRepository.AddAsync(document, cancellationToken);

        // Publish domain event
        var domainEvent = new DocumentUploaded
        {
            DocumentId = document.Id,
            ShipmentId = document.ShipmentId,
            TrackingNumber = document.TrackingNumber,
            DocumentType = document.DocumentType,
            FileName = document.FileName,
            UploadedBy = document.UploadedBy
        };
        await _publisher.Publish(domainEvent, cancellationToken);

        return MapToResponse(document);
    }

    private DocumentResponse MapToResponse(Document document)
    {
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
