using MediatR;
using Ship24X7.Shipment.Application.DTOs;

namespace Ship24X7.Shipment.Application.Queries;

/// <summary>
/// Query for retrieving getdashboardsummary data. Defines query parameters and result type.
/// </summary>
public class GetDashboardSummaryQuery : IRequest<DashboardSummaryResponse>
{
}
