using Ship24X7.Shared.Domain;

namespace Ship24X7.Shipment.Domain.Entities;

/// <summary>
/// Domain entity representing an individual item or package within a shipment.
/// Contains item description, dimensions, weight, and package type for accurate pricing and handling.
/// Multiple items can be included in a single shipment for consolidated shipping.
/// </summary>
public class ShipmentItem : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the shipment item.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the shipment ID this item belongs to.
    /// Links item to parent shipment for grouping and tracking.
    /// </summary>
    public Guid ShipmentId { get; set; }
    
    /// <summary>
    /// Gets or sets the description of the item contents.
    /// Used for customs declarations, handling instructions, and delivery verification.
    /// Should be specific enough for identification (e.g., "Electronics - Laptop", "Clothing - 5 Shirts").
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the quantity of identical items in this entry.
    /// Allows multiple units of the same item to be grouped together.
    /// Total weight = Weight × Quantity.
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Gets or sets the weight of a single unit in kilograms.
    /// Used for total weight calculation and pricing.
    /// Total item weight = Weight × Quantity.
    /// </summary>
    public decimal Weight { get; set; }
    
    /// <summary>
    /// Gets or sets the length of the package in centimeters.
    /// Used for volumetric weight calculation: (Length × Width × Height) / 5000.
    /// Longest dimension of the package.
    /// </summary>
    public decimal Length { get; set; }
    
    /// <summary>
    /// Gets or sets the width of the package in centimeters.
    /// Used for volumetric weight calculation and space planning.
    /// Middle dimension of the package.
    /// </summary>
    public decimal Width { get; set; }
    
    /// <summary>
    /// Gets or sets the height of the package in centimeters.
    /// Used for volumetric weight calculation and stacking decisions.
    /// Shortest dimension of the package.
    /// </summary>
    public decimal Height { get; set; }
    
    /// <summary>
    /// Gets or sets the package type for handling classification.
    /// Common types: "Box", "Envelope", "Tube", "Pallet", "Bag".
    /// Determines handling requirements and loading procedures.
    /// </summary>
    public string PackageType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the shipment navigation property.
    /// Links back to parent shipment containing sender, receiver, and delivery information.
    /// </summary>
    public Shipment Shipment { get; set; } = null!;
}
