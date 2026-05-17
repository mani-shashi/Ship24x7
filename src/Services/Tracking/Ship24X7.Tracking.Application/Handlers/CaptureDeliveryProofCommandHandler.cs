using MediatR;
using Ship24X7.Tracking.Application.Commands;
using Ship24X7.Tracking.Application.DTOs;
using Ship24X7.Tracking.Application.Interfaces;
using Ship24X7.Tracking.Domain.Entities;
using Ship24X7.Tracking.Domain.Enums;
using Ship24X7.Tracking.Domain.Events;
using Ship24X7.Tracking.Domain.ValueObjects;

namespace Ship24X7.Tracking.Application.Handlers;

/// <summary>
/// Handles delivery proof capture by validating shipment status, uploading signature and photo proof, recording GPS coordinates, and creating delivery tracking event.
/// Implements proof of delivery (POD) workflow with signature capture, photo evidence, and geolocation verification.
/// </summary>
public class CaptureDeliveryProofCommandHandler : IRequestHandler<CaptureDeliveryProofCommand, DeliveryProofResponse>
{
    private readonly IDeliveryProofRepository _deliveryProofRepository;
    private readonly ITrackingEventRepository _trackingEventRepository;
    private readonly IDocumentStorageService _documentStorageService;
    private readonly IPublisher _publisher;

    /// <summary>
    /// Initializes a new instance of the CaptureDeliveryProofCommandHandler class.
    /// </summary>
    /// <param name="deliveryProofRepository">Repository for delivery proof operations.</param>
    /// <param name="trackingEventRepository">Repository for tracking event operations.</param>
    /// <param name="documentStorageService">Service for uploading signature and photo files.</param>
    /// <param name="publisher">MediatR publisher for domain events.</param>
    public CaptureDeliveryProofCommandHandler(
        IDeliveryProofRepository deliveryProofRepository,
        ITrackingEventRepository trackingEventRepository,
        IDocumentStorageService documentStorageService,
        IPublisher publisher)
    {
        _deliveryProofRepository = deliveryProofRepository;
        _trackingEventRepository = trackingEventRepository;
        _documentStorageService = documentStorageService;
        _publisher = publisher;
    }

    /// <summary>
    /// Captures delivery proof with signature, photo, and GPS coordinates.
    /// Process flow:
    /// 1. Validates GPS coordinates using GpsCoordinates value object
    /// 2. Retrieves latest tracking event to verify shipment status
    /// 3. Validates shipment is in OutForDelivery status (cannot capture proof for other statuses)
    /// 4. Decodes base64 signature image and uploads to blob storage with timestamped filename
    /// 5. Decodes base64 photo proof and uploads to blob storage with timestamped filename
    /// 6. Creates DeliveryProof entity with recipient name, delivery date, image URLs, GPS coordinates, notes, and deliverer ID
    /// 7. Persists delivery proof to database
    /// 8. Records Delivered tracking event with recipient name and GPS location
    /// 9. Publishes ShipmentDelivered domain event for notifications and downstream processing
    /// 10. Returns delivery proof response with all captured information
    /// This provides complete proof of delivery with multiple verification methods (signature, photo, GPS).
    /// </summary>
    /// <param name="request">Capture delivery proof command with shipment ID, tracking number, recipient, signature/photo images, GPS coordinates, and deliverer ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Delivery proof response with ID, shipment details, recipient, delivery date, image URLs, and GPS coordinates.</returns>
    /// <exception cref="InvalidOperationException">Thrown when shipment is not in OutForDelivery status.</exception>
    public async Task<DeliveryProofResponse> Handle(CaptureDeliveryProofCommand request, CancellationToken cancellationToken)
    {
        // Validate GPS coordinates
        var gpsCoordinates = GpsCoordinates.Create(request.Latitude, request.Longitude);

        // Check if shipment is in OutForDelivery status
        var latestEvent = await _trackingEventRepository.GetLatestByShipmentIdAsync(request.ShipmentId, cancellationToken);
        if (latestEvent == null || latestEvent.Status != ShipmentStatus.OutForDelivery)
        {
            throw new InvalidOperationException("Shipment must be in OutForDelivery status to capture delivery proof");
        }

        // Upload signature image
        var signatureBytes = Convert.FromBase64String(request.SignatureImageBase64);
        var signatureUrl = await _documentStorageService.UploadDocumentAsync(
            $"{request.TrackingNumber}_signature_{DateTime.UtcNow:yyyyMMddHHmmss}.png",
            signatureBytes,
            "image/png",
            cancellationToken);

        // Upload photo proof
        var photoBytes = Convert.FromBase64String(request.PhotoProofBase64);
        var photoUrl = await _documentStorageService.UploadDocumentAsync(
            $"{request.TrackingNumber}_photo_{DateTime.UtcNow:yyyyMMddHHmmss}.jpg",
            photoBytes,
            "image/jpeg",
            cancellationToken);

        // Create delivery proof
        var deliveryProof = new DeliveryProof
        {
            Id = Guid.NewGuid(),
            ShipmentId = request.ShipmentId,
            TrackingNumber = request.TrackingNumber,
            ReceivedBy = request.ReceivedBy,
            DeliveryDate = request.DeliveryDate,
            SignatureImageUrl = signatureUrl,
            PhotoProofUrl = photoUrl,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Notes = request.Notes,
            DeliveredBy = request.DeliveredBy,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.DeliveredBy
        };

        await _deliveryProofRepository.AddAsync(deliveryProof, cancellationToken);

        // Record tracking event for delivery
        var trackingEvent = new TrackingEvent
        {
            Id = Guid.NewGuid(),
            ShipmentId = request.ShipmentId,
            TrackingNumber = request.TrackingNumber,
            Status = ShipmentStatus.Delivered,
            Description = $"Delivered to {request.ReceivedBy}",
            Location = $"GPS: {gpsCoordinates}",
            EventTimestamp = request.DeliveryDate,
            IsException = false,
            RecordedBy = request.DeliveredBy.ToString(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.DeliveredBy
        };
        await _trackingEventRepository.AddAsync(trackingEvent, cancellationToken);

        // Publish ShipmentDelivered event
        var domainEvent = new ShipmentDelivered
        {
            ShipmentId = deliveryProof.ShipmentId,
            TrackingNumber = deliveryProof.TrackingNumber,
            ReceivedBy = deliveryProof.ReceivedBy,
            DeliveryDate = deliveryProof.DeliveryDate,
            Latitude = deliveryProof.Latitude,
            Longitude = deliveryProof.Longitude,
            DeliveredBy = deliveryProof.DeliveredBy
        };
        await _publisher.Publish(domainEvent, cancellationToken);

        return MapToResponse(deliveryProof);
    }

    /// <summary>
    /// Maps delivery proof entity to response DTO.
    /// </summary>
    /// <param name="deliveryProof">Delivery proof entity from database.</param>
    /// <returns>Delivery proof response DTO for API response.</returns>
    private DeliveryProofResponse MapToResponse(DeliveryProof deliveryProof)
    {
        return new DeliveryProofResponse
        {
            Id = deliveryProof.Id,
            ShipmentId = deliveryProof.ShipmentId,
            TrackingNumber = deliveryProof.TrackingNumber,
            ReceivedBy = deliveryProof.ReceivedBy,
            DeliveryDate = deliveryProof.DeliveryDate,
            SignatureImageUrl = deliveryProof.SignatureImageUrl,
            PhotoProofUrl = deliveryProof.PhotoProofUrl,
            Latitude = deliveryProof.Latitude,
            Longitude = deliveryProof.Longitude,
            Notes = deliveryProof.Notes
        };
    }
}
