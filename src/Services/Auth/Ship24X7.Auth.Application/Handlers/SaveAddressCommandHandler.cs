using MediatR;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.DTOs;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Handles create and update operations for user address book entries.
/// When AddressId is null a new address is created; otherwise the existing one is updated.
/// If IsDefault is true, all other addresses for the user are cleared of the default flag first.
/// </summary>
public class SaveAddressCommandHandler : IRequestHandler<SaveAddressCommand, UserAddressDto>
{
    private readonly IUserAddressRepository _addressRepository;

    public SaveAddressCommandHandler(IUserAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<UserAddressDto> Handle(SaveAddressCommand request, CancellationToken cancellationToken)
    {
        // If this address should be the default, clear existing defaults first
        if (request.IsDefault)
            await _addressRepository.ClearDefaultAsync(request.UserId);

        UserAddress address;

        if (request.AddressId.HasValue)
        {
            // Update existing
            address = await _addressRepository.GetByIdAsync(request.AddressId.Value)
                ?? throw new InvalidOperationException("Address not found");

            if (address.UserId != request.UserId)
                throw new UnauthorizedAccessException("Address does not belong to this user");

            address.Label = request.Label.Trim();
            address.ContactName = request.ContactName.Trim();
            address.ContactPhone = request.ContactPhone.Trim();
            address.AddressLine1 = request.AddressLine1.Trim();
            address.AddressLine2 = request.AddressLine2?.Trim();
            address.City = request.City.Trim();
            address.State = request.State.Trim();
            address.PostalCode = request.PostalCode.Trim();
            address.Country = request.Country.Trim();
            address.Type = request.Type;
            address.IsDefault = request.IsDefault;
            address.UpdatedAt = DateTime.UtcNow;
            address.UpdatedBy = request.UserId;

            await _addressRepository.UpdateAsync(address);
        }
        else
        {
            // Create new
            address = new UserAddress
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Label = request.Label.Trim(),
                ContactName = request.ContactName.Trim(),
                ContactPhone = request.ContactPhone.Trim(),
                AddressLine1 = request.AddressLine1.Trim(),
                AddressLine2 = request.AddressLine2?.Trim(),
                City = request.City.Trim(),
                State = request.State.Trim(),
                PostalCode = request.PostalCode.Trim(),
                Country = request.Country.Trim(),
                Type = request.Type,
                IsDefault = request.IsDefault,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.UserId
            };

            await _addressRepository.AddAsync(address);
        }

        return new UserAddressDto
        {
            Id = address.Id,
            Label = address.Label,
            ContactName = address.ContactName,
            ContactPhone = address.ContactPhone,
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            Country = address.Country,
            Type = address.Type,
            IsDefault = address.IsDefault
        };
    }
}
