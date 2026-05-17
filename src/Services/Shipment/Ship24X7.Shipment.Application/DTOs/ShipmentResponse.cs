using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Application.DTOs;

/// <summary>
/// Data transfer object for Shipment data. Used for API responses and data serialization.
/// </summary>
public class ShipmentResponse
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets the trackingNumber.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the customerId.
    /// </summary>
    public Guid CustomerId { get; set; }
    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public ShipmentStatus Status { get; set; }
    /// <summary>
    /// Gets or sets the sender address.
    /// </summary>
    public AddressDto SenderAddress { get; set; } = null!;
    /// <summary>
    /// Gets or sets the receiver address.
    /// </summary>
    public AddressDto ReceiverAddress { get; set; } = null!;
    /// <summary>
    /// Gets or sets the service details.
    /// </summary>
    public ServiceRateDto ServiceRate { get; set; } = null!;
    /// <summary>
    /// Gets or sets the shipment items.
    /// </summary>
    public List<ShipmentItemResponseDto> Items { get; set; } = new();
    /// <summary>
    /// Gets or sets the actualweight.
    /// </summary>
    public decimal ActualWeight { get; set; }
    /// <summary>
    /// Gets or sets the volumetricweight.
    /// </summary>
    public decimal VolumetricWeight { get; set; }
    /// <summary>
    /// Gets or sets the chargeableweight.
    /// </summary>
    public decimal ChargeableWeight { get; set; }
    /// <summary>
    /// Gets or sets the baserate.
    /// </summary>
    public decimal BaseRate { get; set; }
    /// <summary>
    /// Gets or sets the fuelsurcharge.
    /// </summary>
    public decimal FuelSurcharge { get; set; }
    /// <summary>
    /// Gets or sets the insurancecost.
    /// </summary>
    public decimal InsuranceCost { get; set; }
    /// <summary>
    /// Gets or sets the totalcost.
    /// </summary>
    public decimal TotalCost { get; set; }
    /// <summary>
    /// Gets or sets the currency.
    /// </summary>
    public string Currency { get; set; } = "INR";
    /// <summary>
    /// Gets or sets the estimateddeliverydate.
    /// </summary>
    public DateTime EstimatedDeliveryDate { get; set; }
    /// <summary>
    /// Gets or sets the actualdeliverydate.
    /// </summary>
    public DateTime? ActualDeliveryDate { get; set; }
    /// <summary>
    /// Gets or sets whether the shipment is fragile.
    /// </summary>
    public bool IsFragile { get; set; }
    /// <summary>
    /// Gets or sets whether refrigeration is required.
    /// </summary>
    public bool RequiresRefrigeration { get; set; }
    /// <summary>
    /// Gets or sets the declared value.
    /// </summary>
    public decimal? DeclaredValue { get; set; }
    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// Gets or sets the ID of the scheduled pickup for this shipment, if any.
    /// </summary>
    public Guid? PickupId { get; set; }
    /// <summary>
    /// Gets or sets the ID of the hub where the shipment is currently located.
    /// </summary>
    public Guid? CurrentHubId { get; set; }
}

/// <summary>
/// Address DTO for shipment responses.
/// </summary>
public class AddressDto
{
    public Guid Id { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

/// <summary>
/// Service rate DTO for shipment responses.
/// </summary>
public class ServiceRateDto
{
    public Guid Id { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public int EstimatedDeliveryDays { get; set; }
}

/// <summary>
/// Shipment item DTO for responses.
/// </summary>
public class ShipmentItemResponseDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Weight { get; set; }
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public string PackageType { get; set; } = string.Empty;
}
