using Ship24X7.Shared.Domain;

namespace Ship24X7.Shipment.Domain.Entities;

/// <summary>
/// Domain entity representing a physical address with contact information.
/// Used for both sender and receiver addresses in shipment bookings.
/// Supports geocoding with latitude/longitude for route optimization and delivery tracking.
/// </summary>
public class Address : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the address record.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the contact person's full name at this address.
    /// Used for delivery confirmation and communication.
    /// </summary>
    public string ContactName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the contact phone number for delivery coordination.
    /// Used by drivers to contact recipient or sender for access, directions, or delivery issues.
    /// </summary>
    public string ContactPhone { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the contact email address for notifications.
    /// Used for shipment updates, delivery confirmations, and electronic documentation.
    /// </summary>
    public string ContactEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the primary address line (street address, building number).
    /// Main address component for delivery location identification.
    /// </summary>
    public string AddressLine1 { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the secondary address line (apartment, suite, floor, landmark).
    /// Optional additional address details for precise location.
    /// </summary>
    public string AddressLine2 { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the city or municipality name.
    /// Used for hub assignment and delivery zone determination.
    /// </summary>
    public string City { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the state or province name.
    /// Used for regional routing and regulatory compliance.
    /// </summary>
    public string State { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the postal code or ZIP code.
    /// Used for address validation, zone-based pricing, and delivery routing.
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the country name or code.
    /// Determines international vs domestic shipping and customs requirements.
    /// </summary>
    public string Country { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the GPS latitude coordinate of the address.
    /// Optional geocoded location for precise delivery tracking and route optimization.
    /// </summary>
    public decimal? Latitude { get; set; }
    
    /// <summary>
    /// Gets or sets the GPS longitude coordinate of the address.
    /// Optional geocoded location for precise delivery tracking and route optimization.
    /// </summary>
    public decimal? Longitude { get; set; }
}
