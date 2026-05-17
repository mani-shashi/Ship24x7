using Ship24X7.Shared.Domain;

namespace Ship24X7.Shipment.Domain.Entities;

/// <summary>
/// Domain entity representing a shipping service rate configuration.
/// Defines pricing structure, delivery time, and service characteristics for different shipping options.
/// Supports multiple service types (Express, Standard, Economy) with configurable rates and surcharges.
/// </summary>
public class ServiceRate : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the service rate.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets or sets the service type code for categorization.
    /// Typically "EXPRESS", "STANDARD", or "ECONOMY" for different speed/price tiers.
    /// </summary>
    public string ServiceType { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the customer-facing service name.
    /// Descriptive name shown to customers (e.g., "Express Delivery", "Standard Shipping", "Economy Saver").
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the base rate per kilogram for this service.
    /// Foundation of pricing calculation: BaseRate = ChargeableWeight × BaseRatePerKg.
    /// </summary>
    public decimal BaseRatePerKg { get; set; }
    
    /// <summary>
    /// Gets or sets the fuel surcharge percentage applied to base rate.
    /// Variable surcharge to account for fuel cost fluctuations.
    /// FuelSurcharge = BaseRate × (FuelSurchargePercent / 100).
    /// </summary>
    public decimal FuelSurchargePercent { get; set; }
    
    /// <summary>
    /// Gets or sets the minimum charge for this service.
    /// Ensures profitability for small/light shipments.
    /// Final cost is max(CalculatedCost, MinimumCharge).
    /// </summary>
    public decimal MinimumCharge { get; set; }
    
    /// <summary>
    /// Gets or sets the estimated delivery time in days.
    /// Used to calculate EstimatedDeliveryDate = BookingDate + EstimatedDeliveryDays.
    /// Varies by service type (Express: 1-2 days, Standard: 3-5 days, Economy: 5-7 days).
    /// </summary>
    public int EstimatedDeliveryDays { get; set; }
    
    /// <summary>
    /// Gets or sets whether this service rate is currently active and available for booking.
    /// Inactive rates are hidden from customers and excluded from rate calculations.
    /// Can be deactivated for seasonal changes, service discontinuation, or pricing updates.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
