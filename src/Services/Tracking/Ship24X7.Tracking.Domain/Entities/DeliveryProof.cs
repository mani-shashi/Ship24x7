using Ship24X7.Shared.Domain;
using Ship24X7.Tracking.Domain.ValueObjects;

namespace Ship24X7.Tracking.Domain.Entities;

/// <summary>
/// Domain entity representing proof of delivery (POD) for completed shipments.
/// Stores signature image, photo proof, GPS coordinates, recipient information, and delivery timestamp.
/// Provides legal evidence of successful delivery with multiple verification methods.
/// </summary>
public class DeliveryProof : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the delivery proof record.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the shipment ID this delivery proof belongs to.
    /// Links proof to internal shipment record.
    /// </summary>
    public Guid ShipmentId { get; set; }
    
    /// <summary>
    /// Gets or sets the tracking number for public shipment identification.
    /// Enables customer access to delivery proof using tracking number.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the name of the person who received the package.
    /// Captured from recipient signature or driver input at delivery.
    /// </summary>
    public string ReceivedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the date and time when delivery was completed.
    /// Represents actual delivery timestamp, not when proof was uploaded.
    /// </summary>
    public DateTime DeliveryDate { get; set; }
    
    /// <summary>
    /// Gets or sets the URL to the recipient's signature image.
    /// Stored in blob storage, typically PNG format captured on mobile device.
    /// </summary>
    public string SignatureImageUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the URL to the photo proof of delivery.
    /// Stored in blob storage, typically JPEG format showing package at delivery location.
    /// </summary>
    public string PhotoProofUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the GPS latitude coordinate where delivery occurred.
    /// Validates delivery location and provides geolocation proof.
    /// </summary>
    public decimal Latitude { get; set; }
    
    /// <summary>
    /// Gets or sets the GPS longitude coordinate where delivery occurred.
    /// Validates delivery location and provides geolocation proof.
    /// </summary>
    public decimal Longitude { get; set; }
    
    /// <summary>
    /// Gets or sets optional notes about the delivery.
    /// Can include special instructions followed, delivery location details, or recipient comments.
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the driver who delivered the package.
    /// Links delivery to specific driver for accountability and performance tracking.
    /// </summary>
    public Guid DeliveredBy { get; set; }
}
