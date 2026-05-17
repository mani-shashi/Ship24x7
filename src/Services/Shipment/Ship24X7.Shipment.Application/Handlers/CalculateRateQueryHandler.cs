using MediatR;
using Ship24X7.Shipment.Application.DTOs;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Application.Queries;

namespace Ship24X7.Shipment.Application.Handlers;

/// <summary>
/// Query for retrieving calculateratehandler data. Defines query parameters and result type.
/// </summary>
public class CalculateRateQueryHandler : IRequestHandler<CalculateRateQuery, List<RateCalculationResponse>>
{
    private readonly IRateCalculationService _rateCalculationService;

    public CalculateRateQueryHandler(IRateCalculationService rateCalculationService)
    {
        _rateCalculationService = rateCalculationService;
    }

    public async Task<List<RateCalculationResponse>> Handle(CalculateRateQuery request, CancellationToken cancellationToken)
    {
        return await _rateCalculationService.CalculateRatesAsync(
            request.ActualWeight,
            request.Length,
            request.Width,
            request.Height,
            request.ServiceType,
            request.DeclaredValue);
    }
}
