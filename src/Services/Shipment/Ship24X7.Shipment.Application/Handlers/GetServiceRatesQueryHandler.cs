using MediatR;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Application.Queries;
using Ship24X7.Shipment.Domain.Entities;

namespace Ship24X7.Shipment.Application.Handlers;

/// <summary>
/// Returns all active service rates, optionally filtered by service type.
/// Uses IServiceRateRepository to keep the Application layer decoupled from Infrastructure.
/// </summary>
public class GetServiceRatesQueryHandler : IRequestHandler<GetServiceRatesQuery, List<ServiceRate>>
{
    private readonly IServiceRateRepository _serviceRateRepository;

    public GetServiceRatesQueryHandler(IServiceRateRepository serviceRateRepository)
    {
        _serviceRateRepository = serviceRateRepository;
    }

    public async Task<List<ServiceRate>> Handle(GetServiceRatesQuery request, CancellationToken cancellationToken)
    {
        return await _serviceRateRepository.GetActiveRatesAsync();
    }
}
