using MediatR;
using Ship24X7.Shipment.Application.Commands;
using Ship24X7.Shipment.Application.Interfaces;

namespace Ship24X7.Shipment.Application.Handlers;

/// <summary>
/// Handles RetireServiceRateCommand by soft-deleting a ServiceRate (sets IsActive = false).
/// The record is retained for historical rate calculations on existing shipments.
/// Throws KeyNotFoundException if the rate does not exist.
/// </summary>
public class RetireServiceRateCommandHandler : IRequestHandler<RetireServiceRateCommand, bool>
{
    private readonly IServiceRateRepository _rateRepository;

    public RetireServiceRateCommandHandler(IServiceRateRepository rateRepository)
    {
        _rateRepository = rateRepository;
    }

    public async Task<bool> Handle(RetireServiceRateCommand request, CancellationToken cancellationToken)
    {
        var rate = await _rateRepository.GetByIdAsync(request.RateId);
        if (rate == null)
            throw new KeyNotFoundException($"Service rate configuration {request.RateId} not found");

        rate.IsActive = false;
        await _rateRepository.UpdateAsync(rate);
        return true;
    }
}
