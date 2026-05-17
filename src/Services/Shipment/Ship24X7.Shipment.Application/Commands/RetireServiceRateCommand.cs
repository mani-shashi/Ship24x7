using MediatR;

namespace Ship24X7.Shipment.Application.Commands;

/// <summary>
/// Command to soft-delete (retire) a shipping service rate configuration.
/// Sets IsActive to false — the record is retained for historical rate calculations.
/// </summary>
public class RetireServiceRateCommand : IRequest<bool>
{
    public Guid RateId { get; set; }
}
