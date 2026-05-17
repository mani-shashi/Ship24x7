using MediatR;
using Microsoft.Extensions.Logging;
using Ship24X7.Shipment.Application.DTOs;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Application.Queries;

namespace Ship24X7.Shipment.Application.Handlers;

/// <summary>
/// Returns the full status history for a shipment, ordered oldest-first.
/// </summary>
public class GetShipmentStatusHistoryQueryHandler
    : IRequestHandler<GetShipmentStatusHistoryQuery, List<ShipmentStatusHistoryDto>>
{
    private readonly IShipmentStatusHistoryRepository _historyRepository;
    private readonly ILogger<GetShipmentStatusHistoryQueryHandler> _logger;

    public GetShipmentStatusHistoryQueryHandler(
        IShipmentStatusHistoryRepository historyRepository,
        ILogger<GetShipmentStatusHistoryQueryHandler> logger)
    {
        _historyRepository = historyRepository;
        _logger = logger;
    }

    public async Task<List<ShipmentStatusHistoryDto>> Handle(
        GetShipmentStatusHistoryQuery request, CancellationToken cancellationToken)
    {
        var history = await _historyRepository.GetByShipmentIdAsync(request.ShipmentId);

        return history.Select(h => new ShipmentStatusHistoryDto
        {
            Id = h.Id,
            ShipmentId = h.ShipmentId,
            FromStatus = h.FromStatus.ToString(),
            ToStatus = h.ToStatus.ToString(),
            ChangedBy = h.ChangedBy,
            Reason = h.Reason,
            ChangedAt = h.ChangedAt,
            HubId = h.HubId
        }).ToList();
    }
}
