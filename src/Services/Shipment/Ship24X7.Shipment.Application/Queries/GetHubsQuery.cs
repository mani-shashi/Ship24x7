using MediatR;
using Ship24X7.Shipment.Application.DTOs;

namespace Ship24X7.Shipment.Application.Queries;

/// <summary>
/// Returns all hubs. Pass activeOnly = false to include deactivated hubs (admin use).
/// </summary>
public class GetHubsQuery : IRequest<List<HubDto>>
{
    /// <summary>When true (default), only active hubs are returned.</summary>
    public bool ActiveOnly { get; set; } = true;
}
