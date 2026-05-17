using MediatR;
using Ship24X7.Shipment.Application.DTOs;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Application.Queries;
using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Application.Handlers;

/// <summary>
/// Query for retrieving getdashboardsummaryhandler data. Defines query parameters and result type.
/// </summary>
public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryResponse>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IPickupRepository _pickupRepository;

    public GetDashboardSummaryQueryHandler(
        IShipmentRepository shipmentRepository,
        IPickupRepository pickupRepository)
    {
        _shipmentRepository = shipmentRepository;
        _pickupRepository = pickupRepository;
    }

    public async Task<DashboardSummaryResponse> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var todayBookingCount = await _shipmentRepository.GetTodayBookingCountAsync();
        var pendingPickupCount = await _pickupRepository.GetPendingPickupCountAsync();
        var inTransitCount = await _shipmentRepository.GetCountByStatusAsync(ShipmentStatus.InTransit);
        var outForDeliveryCount = await _shipmentRepository.GetCountByStatusAsync(ShipmentStatus.OutForDelivery);
        var deliveredTodayCount = await _shipmentRepository.GetDeliveredTodayCountAsync();

        return new DashboardSummaryResponse
        {
            TodayBookingCount = todayBookingCount,
            PendingPickupCount = pendingPickupCount,
            InTransitCount = inTransitCount,
            OutForDeliveryCount = outForDeliveryCount,
            DeliveredTodayCount = deliveredTodayCount
        };
    }
}
