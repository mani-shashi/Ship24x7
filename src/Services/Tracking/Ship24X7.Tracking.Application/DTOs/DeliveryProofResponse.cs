namespace Ship24X7.Tracking.Application.DTOs;

/// <summary>
/// Data transfer object for DeliveryProof data. Used for API responses and data serialization.
/// </summary>
public class DeliveryProofResponse
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
    /// Gets or sets the receivedby.
    /// </summary>
    public string ReceivedBy { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the deliverydate.
    /// </summary>
    public DateTime DeliveryDate { get; set; }
    /// <summary>
    /// Gets or sets the signatureimageurl.
    /// </summary>
    public string SignatureImageUrl { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the photoproofurl.
    /// </summary>
    public string PhotoProofUrl { get; set; } = string.Empty;
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
}
