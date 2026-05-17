using Ship24X7.Shipment.Application.DTOs;
using Ship24X7.Shipment.Application.Interfaces;

namespace Ship24X7.Shipment.Infrastructure.Services;

/// <summary>
/// Rate calculation service for computing shipping costs based on weight, dimensions, and service type.
/// Implements volumetric weight calculation, chargeable weight determination, and multi-service rate comparison.
/// Applies base rates, fuel surcharges, insurance costs, and minimum charge enforcement.
/// </summary>
public class RateCalculationService : IRateCalculationService
{
    private readonly IServiceRateRepository _serviceRateRepository;
    private const decimal VolumetricDivisor = 5000m; // Standard volumetric divisor for air freight
    private const decimal InsuranceRate = 0.01m; // 1% of declared value

    /// <summary>
    /// Initializes a new instance of the <see cref="RateCalculationService"/> class.
    /// </summary>
    /// <param name="serviceRateRepository">Repository for retrieving active service rates.</param>
    public RateCalculationService(IServiceRateRepository serviceRateRepository)
    {
        _serviceRateRepository = serviceRateRepository;
    }

    /// <summary>
    /// Calculates shipping rates for all available services or a specific service type.
    /// Process flow:
    /// 1. Retrieves active service rates from database (filtered by service type if specified)
    /// 2. Calculates volumetric weight: (Length × Width × Height) / 5000
    /// 3. Determines chargeable weight: max(ActualWeight, VolumetricWeight)
    /// 4. For each service rate:
    ///    a. Calculates base rate: ChargeableWeight × BaseRatePerKg
    ///    b. Calculates fuel surcharge: BaseRate × (FuelSurchargePercent / 100)
    ///    c. Calculates insurance cost: DeclaredValue × 1% (if declared value provided)
    ///    d. Calculates total cost: BaseRate + FuelSurcharge + InsuranceCost
    ///    e. Enforces minimum charge: max(TotalCost, MinimumCharge)
    ///    f. Calculates estimated delivery date: CurrentDate + EstimatedDeliveryDays
    /// 5. Returns results sorted by total cost (cheapest first)
    /// Volumetric weight accounts for package size to prevent undercharging for large, light items.
    /// </summary>
    /// <param name="actualWeight">Physical weight of the shipment in kilograms.</param>
    /// <param name="length">Package length in centimeters (longest dimension).</param>
    /// <param name="width">Package width in centimeters (middle dimension).</param>
    /// <param name="height">Package height in centimeters (shortest dimension).</param>
    /// <param name="serviceType">Optional service type filter (e.g., "EXPRESS", "STANDARD", "ECONOMY"). If null, calculates for all active services.</param>
    /// <param name="declaredValue">Optional declared value for insurance calculation. If null, no insurance cost is added.</param>
    /// <returns>List of rate calculation responses sorted by total cost, showing pricing breakdown for each service.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no active service rates are found.</exception>
    public async Task<List<RateCalculationResponse>> CalculateRatesAsync(
        decimal actualWeight,
        decimal length,
        decimal width,
        decimal height,
        string? serviceType = null,
        decimal? declaredValue = null)
    {
        // Get active service rates
        var serviceRates = await _serviceRateRepository.GetActiveRatesAsync(serviceType);
        
        if (!serviceRates.Any())
            throw new InvalidOperationException("No active service rates found");

        // Calculate volumetric weight
        var volumetricWeight = (length * width * height) / VolumetricDivisor;
        
        // Calculate chargeable weight (max of actual and volumetric)
        var chargeableWeight = Math.Max(actualWeight, volumetricWeight);

        var results = new List<RateCalculationResponse>();

        foreach (var rate in serviceRates)
        {
            // Calculate base rate
            var baseRate = rate.BaseRatePerKg * chargeableWeight;
            
            // Calculate fuel surcharge
            var fuelSurcharge = baseRate * (rate.FuelSurchargePercent / 100m);
            
            // Calculate insurance cost if declared value provided
            var insuranceCost = declaredValue.HasValue ? declaredValue.Value * InsuranceRate : 0m;
            
            // Calculate total cost (ensure it meets minimum charge)
            var totalCost = Math.Max(baseRate + fuelSurcharge + insuranceCost, rate.MinimumCharge);
            
            // Calculate estimated delivery date
            var estimatedDeliveryDate = DateTime.UtcNow.AddDays(rate.EstimatedDeliveryDays);

            results.Add(new RateCalculationResponse
            {
                ServiceRateId = rate.Id,
                ServiceType = rate.ServiceType,
                ServiceName = rate.ServiceName,
                ActualWeight = Math.Round(actualWeight, 2),
                VolumetricWeight = Math.Round(volumetricWeight, 2),
                ChargeableWeight = Math.Round(chargeableWeight, 2),
                BaseRate = Math.Round(baseRate, 2),
                FuelSurcharge = Math.Round(fuelSurcharge, 2),
                InsuranceCost = Math.Round(insuranceCost, 2),
                TotalCost = Math.Round(totalCost, 2),
                Currency = "INR",
                EstimatedDeliveryDays = rate.EstimatedDeliveryDays,
                EstimatedDeliveryDate = estimatedDeliveryDate
            });
        }

        return results.OrderBy(r => r.TotalCost).ToList();
    }
}
