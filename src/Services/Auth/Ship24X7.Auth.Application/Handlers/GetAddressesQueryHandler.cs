using MediatR;
using Ship24X7.Auth.Application.DTOs;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Application.Queries;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Returns all saved addresses for the authenticated user.
/// </summary>
public class GetAddressesQueryHandler : IRequestHandler<GetAddressesQuery, IEnumerable<UserAddressDto>>
{
    private readonly IUserAddressRepository _addressRepository;

    public GetAddressesQueryHandler(IUserAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<IEnumerable<UserAddressDto>> Handle(GetAddressesQuery request, CancellationToken cancellationToken)
    {
        var addresses = await _addressRepository.GetByUserIdAsync(request.UserId);

        return addresses.Select(a => new UserAddressDto
        {
            Id = a.Id,
            Label = a.Label,
            ContactName = a.ContactName,
            ContactPhone = a.ContactPhone,
            AddressLine1 = a.AddressLine1,
            AddressLine2 = a.AddressLine2,
            City = a.City,
            State = a.State,
            PostalCode = a.PostalCode,
            Country = a.Country,
            Type = a.Type,
            IsDefault = a.IsDefault
        });
    }
}
