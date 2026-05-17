using Ship24X7.Tracking.Domain.Enums;

namespace Ship24X7.Tracking.Application.DTOs;

/// <summary>
/// Data transfer object for Document data. Used for API responses and data serialization.
/// </summary>
public class DocumentResponse
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }
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
    /// <summary>
    /// Gets or sets the fileurl.
    /// </summary>
    public string FileUrl { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Content type.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the filesizebytes.
    /// </summary>
    public long FileSizeBytes { get; set; }
    /// <summary>
    /// Gets or sets the uploadedat.
    /// </summary>
    public DateTime UploadedAt { get; set; }
}
