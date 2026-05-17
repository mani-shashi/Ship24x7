using Ship24X7.Shared.Domain;
using Ship24X7.Tracking.Domain.Enums;

namespace Ship24X7.Tracking.Domain.Events;

/// <summary>
/// Domain event raised when DocumentUploaded occurs. Used for event-driven architecture and integration.
/// </summary>
public class DocumentUploaded : BaseDomainEvent
{
    /// <summary>
    /// Gets or sets the documentid.
    /// </summary>
    public Guid DocumentId { get; set; }
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
    /// Gets or sets the uploadedby.
    /// </summary>
    public Guid UploadedBy { get; set; }
}
