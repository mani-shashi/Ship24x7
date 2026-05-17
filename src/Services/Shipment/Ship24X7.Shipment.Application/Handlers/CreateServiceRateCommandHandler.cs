using MediatR;
using Ship24X7.Shipment.Application.Commands;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Domain.Entities;

namespace Ship24X7.Shipment.Application.Handlers;

/// <summary>
/// Handles CreateServiceRateCommand by persisting a new ServiceRate entity.
/// </summary>
public class CreateServiceRateCommandHandler : IRequestHandler<CreateServiceRateCommand, Guid>
{
    private readonly IServiceRateRepository _rateRepository;

    public CreateServiceRateCommandHandler(IServiceRateRepository rateRepository)
    {
        _rateRepository = rateRepository;
    }

    public async Task<Guid> Handle(CreateServiceRateCommand request, CancellationToken cancellationToken)
    {
        var rate = new ServiceRate
        {
            Id = Guid.NewGuid(),
            ServiceType = request.ServiceType,
            ServiceName = request.ServiceName,
            BaseRatePerKg = request.BaseRatePerKg,
            FuelSurchargePercent = request.FuelSurchargePercent,
            MinimumCharge = request.MinimumCharge,
            EstimatedDeliveryDays = request.EstimatedDeliveryDays,
            IsActive = true
        };

        await _rateRepository.AddAsync(rate);
        return rate.Id;
    }
}
