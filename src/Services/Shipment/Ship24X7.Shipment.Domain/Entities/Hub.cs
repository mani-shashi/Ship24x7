using Ship24X7.Shared.Domain;

namespace Ship24X7.Shipment.Domain.Entities;

/// <summary>
/// Domain entity representing a logistics hub or distribution center in the shipping network.
/// Manages hub capacity, location, and operational status for shipment routing and sorting.
/// Hubs serve as origin points, transit points, and destination points in the delivery network.
/// </summary>
public class Hub : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the hub.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the hub name for identification.
    /// Typically includes location (e.g., "Mumbai Central Hub", "Delhi North Hub").
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the unique hub code for routing and tracking.
    /// Short alphanumeric code used in tracking events and routing decisions (e.g., "MUM-C", "DEL-N").
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the primary address line of the hub facility.
    /// Physical location of the distribution center.
    /// </summary>
    public string AddressLine1 { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the secondary address line with additional location details.
    /// Optional facility-specific information.
    /// </summary>
    public string AddressLine2 { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the city where the hub is located.
    /// Used for regional hub assignment and routing decisions.
    /// </summary>
    public string City { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the state or province of the hub location.
    /// Used for inter-state routing and regulatory compliance.
    /// </summary>
    public string State { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the postal code of the hub facility.
    /// Used for address validation and zone-based routing.
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the country where the hub operates.
    /// Determines international vs domestic hub classification.
    /// </summary>
    public string Country { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the GPS latitude coordinate of the hub.
    /// Used for distance calculations, route optimization, and nearest hub assignment.
    /// </summary>
    public decimal Latitude { get; set; }
    
    /// <summary>
    /// Gets or sets the GPS longitude coordinate of the hub.
    /// Used for distance calculations, route optimization, and nearest hub assignment.
    /// </summary>
    public decimal Longitude { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum shipment capacity of the hub.
    /// Defines how many shipments the hub can process simultaneously.
    /// Used for load balancing and capacity planning.
    /// </summary>
    public int Capacity { get; set; }
    
    /// <summary>
    /// Gets or sets the current number of shipments at the hub.
    /// Tracks real-time hub utilization for routing decisions.
    /// Updated as shipments arrive and depart from the hub.
    /// </summary>
    public int CurrentLoad { get; set; }
    
    /// <summary>
    /// Gets or sets whether the hub is currently operational.
    /// Inactive hubs are excluded from routing and assignment decisions.
    /// Can be deactivated for maintenance, capacity issues, or operational changes.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
