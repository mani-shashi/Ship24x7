using MediatR;
using Ship24X7.Shipment.Application.DTOs;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Application.Queries;

namespace Ship24X7.Shipment.Application.Handlers;

/// <summary>
/// Returns a list of hubs projected to HubDto.
/// Keeps the Application layer decoupled from the domain entity shape.
/// </summary>
public class GetHubsQueryHandler : IRequestHandler<GetHubsQuery, List<HubDto>>
{
    private readonly IHubRepository _hubRepository;

    public GetHubsQueryHandler(IHubRepository hubRepository)
    {
        _hubRepository = hubRepository;
    }

    public async Task<List<HubDto>> Handle(GetHubsQuery request, CancellationToken cancellationToken)
    {
        var hubs = await _hubRepository.GetAllAsync(request.ActiveOnly);

        return hubs.Select(h => new HubDto
        {
            Id = h.Id,
            Name = h.Name,
            Code = h.Code,
            City = h.City,
            State = h.State,
            Capacity = h.Capacity,
            CurrentLoad = h.CurrentLoad,
            IsActive = h.IsActive
        }).ToList();
    }
}
