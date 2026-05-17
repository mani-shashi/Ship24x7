using MediatR;

namespace Ship24X7.Shipment.Application.Commands;

/// <summary>
/// Command to create a new shipping service rate configuration.
/// </summary>
public class CreateServiceRateCommand : IRequest<Guid>
{
    public string ServiceType { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public decimal BaseRatePerKg { get; set; }
    public decimal FuelSurchargePercent { get; set; }
    public decimal MinimumCharge { get; set; }
    public int EstimatedDeliveryDays { get; set; }
}
