using MediatR;
using Ship24X7.Shipment.Domain.Entities;

namespace Ship24X7.Shipment.Application.Queries;

public class GetServiceRatesQuery : IRequest<List<ServiceRate>>
{
}
