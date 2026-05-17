using Ship24X7.Shipment.Application.DTOs;

namespace Ship24X7.Shipment.Application.Interfaces;

/// <summary>
/// IRateCalculation service implementation. Provides iratecalculation functionality for the application.
/// </summary>
public interface IRateCalculationService
{
    Task<List<RateCalculationResponse>> CalculateRatesAsync(
        decimal actualWeight,
        decimal length,
        decimal width,
        decimal height,
        string? serviceType = null,
        decimal? declaredValue = null);
}
