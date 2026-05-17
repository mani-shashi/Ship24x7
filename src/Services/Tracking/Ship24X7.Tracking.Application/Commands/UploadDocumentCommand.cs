using MediatR;
using Ship24X7.Tracking.Application.DTOs;
using Ship24X7.Tracking.Domain.Enums;

namespace Ship24X7.Tracking.Application.Commands;

/// <summary>
/// Command for uploaddocument operation. Encapsulates request data and validation rules.
/// </summary>
public class UploadDocumentCommand : IRequest<DocumentResponse>
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
    /// Gets or sets the Document type.
    /// </summary>
    public DocumentType DocumentType { get; set; }
    /// <summary>
    /// Gets or sets the File name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    /// <summary>
    /// Gets or sets the Content type.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the uploadedby.
    /// </summary>
    public Guid UploadedBy { get; set; }
}
