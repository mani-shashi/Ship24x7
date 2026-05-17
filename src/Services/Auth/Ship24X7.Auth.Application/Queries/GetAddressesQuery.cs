using MediatR;
using Ship24X7.Auth.Application.DTOs;

namespace Ship24X7.Auth.Application.Queries;

/// <summary>
/// Query to retrieve all saved addresses for a user.
/// </summary>
public class GetAddressesQuery : IRequest<IEnumerable<UserAddressDto>>
{
    public Guid UserId { get; set; }
}
