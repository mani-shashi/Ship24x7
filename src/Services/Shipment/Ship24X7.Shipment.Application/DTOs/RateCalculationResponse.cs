namespace Ship24X7.Shipment.Application.DTOs;

/// <summary>
/// Data transfer object for RateCalculation data. Used for API responses and data serialization.
/// </summary>
public class RateCalculationResponse
{
    /// <summary>
    /// Gets or sets the servicerateid.
    /// </summary>
    public Guid ServiceRateId { get; set; }
    /// <summary>
    /// Gets or sets the Service type.
    /// </summary>
    public string ServiceType { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Service name.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;
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
    /// Gets or sets the estimateddeliverydays.
    /// </summary>
    public int EstimatedDeliveryDays { get; set; }
    /// <summary>
    /// Gets or sets the estimateddeliverydate.
    /// </summary>
    public DateTime EstimatedDeliveryDate { get; set; }
}
