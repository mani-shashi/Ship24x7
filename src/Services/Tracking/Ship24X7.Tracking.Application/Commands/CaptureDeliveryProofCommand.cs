using MediatR;
using Ship24X7.Tracking.Application.DTOs;

namespace Ship24X7.Tracking.Application.Commands;

/// <summary>
/// Command for capturedeliveryproof operation. Encapsulates request data and validation rules.
/// </summary>
public class CaptureDeliveryProofCommand : IRequest<DeliveryProofResponse>
{
    /// <summary>
    /// Gets or sets the shipmentId.
    /// </summary>
    public Guid ShipmentId { get; set; }
    /// <summary>
    /// Gets or sets the trackingNumber.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the receivedby.
    /// </summary>
    public string ReceivedBy { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the deliverydate.
    /// </summary>
    public DateTime DeliveryDate { get; set; }
    /// <summary>
    /// Gets or sets the signatureImageBase64.
    /// </summary>
    public string SignatureImageBase64 { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the photoproofbase64.
    /// </summary>
    public string PhotoProofBase64 { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the latitude.
    /// </summary>
    public decimal Latitude { get; set; }
    /// <summary>
    /// Gets or sets the longitude.
    /// </summary>
    public decimal Longitude { get; set; }
    /// <summary>
    /// Gets or sets the notes.
    /// </summary>
    public string? Notes { get; set; }
    /// <summary>
    /// Gets or sets the deliveredby.
    /// </summary>
    public Guid DeliveredBy { get; set; }
}
