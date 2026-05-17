using Ship24X7.Shared.Domain;
using Ship24X7.Tracking.Domain.Enums;

namespace Ship24X7.Tracking.Domain.Entities;

/// <summary>
/// Domain entity representing a shipment-related document.
/// Stores metadata for shipping labels, customs declarations, invoices, and other shipment documents.
/// Actual file content is stored in blob storage; this entity tracks file location and metadata.
/// </summary>
public class Document : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the document record.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the shipment ID this document belongs to.
    /// Links document to internal shipment record.
    /// </summary>
    public Guid ShipmentId { get; set; }
    
    /// <summary>
    /// Gets or sets the tracking number for public shipment identification.
    /// Enables customer access to documents using tracking number.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the type of document.
    /// Categorizes document as ShippingLabel, CustomsDeclaration, Invoice, PackingList, or Other.
    /// </summary>
    public DocumentType DocumentType { get; set; }
    
    /// <summary>
    /// Gets or sets the original file name from upload.
    /// Preserves user's original filename for download purposes.
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the URL or identifier for accessing the file in blob storage.
    /// Used to retrieve file content from storage service.
    /// </summary>
    public string FileUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the MIME content type of the file.
    /// Typically "application/pdf", "image/jpeg", or "image/png" for shipment documents.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the file size in bytes.
    /// Used for storage quota management and download size display.
    /// </summary>
    public long FileSizeBytes { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when the document was uploaded.
    /// Tracks document creation time for audit and chronological ordering.
    /// </summary>
    public DateTime UploadedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the user who uploaded the document.
    /// Can be customer, driver, hub staff, or system (Guid.Empty for system-generated documents).
    /// </summary>
    public Guid UploadedBy { get; set; }
}
