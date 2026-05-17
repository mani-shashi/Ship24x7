using MediatR;
using Ship24X7.Shipment.Application.Commands;
using Ship24X7.Shipment.Application.Interfaces;

namespace Ship24X7.Shipment.Application.Handlers;

/// <summary>
/// Handles UpdateServiceRateCommand by updating an existing ServiceRate entity.
/// Throws KeyNotFoundException if the rate does not exist.
/// </summary>
public class UpdateServiceRateCommandHandler : IRequestHandler<UpdateServiceRateCommand, bool>
{
    private readonly IServiceRateRepository _rateRepository;

    public UpdateServiceRateCommandHandler(IServiceRateRepository rateRepository)
    {
        _rateRepository = rateRepository;
    }

    public async Task<bool> Handle(UpdateServiceRateCommand request, CancellationToken cancellationToken)
    {
        var rate = await _rateRepository.GetByIdAsync(request.RateId);
        if (rate == null)
            throw new KeyNotFoundException($"Service rate configuration {request.RateId} not found");

        rate.ServiceName = request.ServiceName;
        rate.BaseRatePerKg = request.BaseRatePerKg;
        rate.FuelSurchargePercent = request.FuelSurchargePercent;
        rate.MinimumCharge = request.MinimumCharge;
        rate.EstimatedDeliveryDays = request.EstimatedDeliveryDays;
        rate.IsActive = request.IsActive;

        await _rateRepository.UpdateAsync(rate);
        return true;
    }
}
