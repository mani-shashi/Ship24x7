using MediatR;

namespace Ship24X7.Shipment.Application.Commands;

/// <summary>
/// Command to update an existing shipping service rate configuration.
/// </summary>
public class UpdateServiceRateCommand : IRequest<bool>
{
    public Guid RateId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal BaseRatePerKg { get; set; }
    public decimal FuelSurchargePercent { get; set; }
    public decimal MinimumCharge { get; set; }
    public int EstimatedDeliveryDays { get; set; }
    public bool IsActive { get; set; }
}
