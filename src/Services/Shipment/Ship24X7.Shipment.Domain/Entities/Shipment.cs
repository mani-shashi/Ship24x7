using Ship24X7.Shared.Domain;
using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Domain.Entities;

/// <summary>
/// Domain entity representing a shipment booking in the Ship24X7 system.
/// Contains complete shipment information including sender/receiver addresses, items, pricing, delivery estimates, and status tracking.
/// Supports special handling requirements (fragile, refrigeration) and idempotency for duplicate prevention.
/// </summary>
public class Shipment : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the shipment.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the unique tracking number for public shipment identification.
    /// Format: SHIP24X7-YYYYMMDDNNNNNN (e.g., SHIP24X7-20260420000001).
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the customer ID who created the shipment booking.
    /// Links shipment to customer account for authorization and billing.
    /// </summary>
    public Guid CustomerId { get; set; }
    
    /// <summary>
    /// Gets or sets the current status of the shipment in its lifecycle.
    /// Tracks progression from Draft → Booked → Paid → PickedUp → InTransit → OutForDelivery → Delivered.
    /// </summary>
    public ShipmentStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets the sender address ID.
    /// References the Address entity containing sender contact and location information.
    /// </summary>
    public Guid SenderAddressId { get; set; }
    
    /// <summary>
    /// Gets or sets the receiver address ID.
    /// References the Address entity containing receiver contact and location information.
    /// </summary>
    public Guid ReceiverAddressId { get; set; }
    
    /// <summary>
    /// Gets or sets the service rate ID used for this shipment.
    /// Determines service type (Express, Standard, Economy) and pricing structure.
    /// </summary>
    public Guid ServiceRateId { get; set; }
    
    /// <summary>
    /// Gets or sets the actual physical weight of the shipment in kilograms.
    /// Measured weight of all items combined.
    /// </summary>
    public decimal ActualWeight { get; set; }
    
    /// <summary>
    /// Gets or sets the volumetric weight calculated from package dimensions.
    /// Formula: (Length × Width × Height) / 5000 for dimensional weight pricing.
    /// </summary>
    public decimal VolumetricWeight { get; set; }
    
    /// <summary>
    /// Gets or sets the chargeable weight used for pricing calculation.
    /// Higher of actual weight or volumetric weight to account for space utilization.
    /// </summary>
    public decimal ChargeableWeight { get; set; }
    
    /// <summary>
    /// Gets or sets the base shipping rate before surcharges.
    /// Calculated as: ChargeableWeight × BaseRatePerKg from ServiceRate.
    /// </summary>
    public decimal BaseRate { get; set; }
    
    /// <summary>
    /// Gets or sets the fuel surcharge amount.
    /// Calculated as percentage of base rate to cover fuel cost fluctuations.
    /// </summary>
    public decimal FuelSurcharge { get; set; }
    
    /// <summary>
    /// Gets or sets the insurance cost for declared value coverage.
    /// Typically 1% of declared value for shipment protection.
    /// </summary>
    public decimal InsuranceCost { get; set; }
    
    /// <summary>
    /// Gets or sets the total cost of the shipment.
    /// Sum of BaseRate + FuelSurcharge + InsuranceCost, with minimum charge enforcement.
    /// </summary>
    public decimal TotalCost { get; set; }
    
    /// <summary>
    /// Gets or sets the currency code for pricing (default: "INR").
    /// Supports multi-currency pricing for international shipments.
    /// </summary>
    public string Currency { get; set; } = "INR";
    
    /// <summary>
    /// Gets or sets the estimated delivery date based on service type.
    /// Calculated from booking date + EstimatedDeliveryDays from ServiceRate.
    /// </summary>
    public DateTime EstimatedDeliveryDate { get; set; }
    
    /// <summary>
    /// Gets or sets the actual delivery date when shipment was delivered.
    /// Null until shipment reaches Delivered status.
    /// </summary>
    public DateTime? ActualDeliveryDate { get; set; }
    
    /// <summary>
    /// Gets or sets the origin hub ID where shipment enters the network.
    /// Assigned based on sender's location for routing optimization.
    /// </summary>
    public Guid? OriginHubId { get; set; }
    
    /// <summary>
    /// Gets or sets the destination hub ID for final delivery routing.
    /// Assigned based on receiver's location for last-mile delivery.
    /// </summary>
    public Guid? DestinationHubId { get; set; }
    
    /// <summary>
    /// Gets or sets whether the shipment contains fragile items.
    /// Triggers special handling instructions and careful transport requirements.
    /// </summary>
    public bool IsFragile { get; set; }
    
    /// <summary>
    /// Gets or sets whether the shipment requires refrigeration.
    /// Ensures cold chain logistics for temperature-sensitive items.
    /// </summary>
    public bool RequiresRefrigeration { get; set; }
    
    /// <summary>
    /// Gets or sets the declared value of shipment contents for insurance.
    /// Used to calculate insurance cost and maximum liability coverage.
    /// </summary>
    public decimal? DeclaredValue { get; set; }
    
    /// <summary>
    /// Gets or sets the idempotency key for duplicate request prevention.
    /// Ensures same shipment isn't created multiple times from retried API calls.
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    // ── Hub tracking ──────────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets the ID of the hub where the shipment is currently located.
    /// Updated each time the shipment arrives at or departs from a hub.
    /// Null until the shipment enters the network (PickedUp status).
    /// </summary>
    public Guid? CurrentHubId { get; set; }

    /// <summary>
    /// Navigation property for the hub where the shipment currently resides.
    /// </summary>
    public Hub? CurrentHub { get; set; }

    // ── Delivery OTP ──────────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets the SHA-256 hash of the 6-digit OTP sent to the customer
    /// when the shipment moves to OutForDelivery status.
    /// Never store the raw OTP — only the hash is persisted.
    /// Null until the shipment reaches OutForDelivery.
    /// </summary>
    public string? DeliveryOtpHash { get; set; }

    /// <summary>
    /// Gets or sets the UTC expiry time of the delivery OTP.
    /// OTP is valid for 24 hours from generation.
    /// Null until the shipment reaches OutForDelivery.
    /// </summary>
    public DateTime? OtpExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the ID of the delivery agent assigned for last-mile delivery.
    /// Set when the shipment moves to OutForDelivery status.
    /// </summary>
    public string? DeliveryAgentId { get; set; }
    
    /// <summary>
    /// Gets or sets the sender address navigation property.
    /// Contains complete sender contact information and location details.
    /// </summary>
    public Address SenderAddress { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the receiver address navigation property.
    /// Contains complete receiver contact information and location details.
    /// </summary>
    public Address ReceiverAddress { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the service rate navigation property.
    /// Contains pricing structure and delivery time estimates for selected service.
    /// </summary>
    public ServiceRate ServiceRate { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the origin hub navigation property.
    /// Hub where shipment enters the distribution network.
    /// </summary>
    public Hub? OriginHub { get; set; }
    
    /// <summary>
    /// Gets or sets the destination hub navigation property.
    /// Hub responsible for final delivery to receiver.
    /// </summary>
    public Hub? DestinationHub { get; set; }
    
    /// <summary>
    /// Gets or sets the collection of items included in this shipment.
    /// Each item has description, dimensions, weight, and package type.
    /// </summary>
    public ICollection<ShipmentItem> Items { get; set; } = new List<ShipmentItem>();
    
    /// <summary>
    /// Gets or sets the pickup schedule for this shipment.
    /// Contains pickup date, time slot, confirmation number, and driver assignment.
    /// </summary>
    public Pickup? Pickup { get; set; }

    /// <summary>
    /// Append-only audit log of every status transition for this shipment.
    /// </summary>
    public ICollection<ShipmentStatusHistory> StatusHistory { get; set; } = new List<ShipmentStatusHistory>();
}
