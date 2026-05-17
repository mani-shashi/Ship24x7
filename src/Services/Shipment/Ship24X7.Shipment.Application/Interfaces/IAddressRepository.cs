using Ship24X7.Shipment.Domain.Entities;

namespace Ship24X7.Shipment.Application.Interfaces;

/// <summary>
/// Repository for managing IAddress persistence operations. Handles database CRUD operations and queries.
/// </summary>
public interface IAddressRepository
{
    Task<Address?> GetByIdAsync(Guid id);
    Task<Address> AddAsync(Address address);
    Task UpdateAsync(Address address);
}
