using Microsoft.EntityFrameworkCore;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Domain.Entities;
using Ship24X7.Shipment.Infrastructure.Persistence;

namespace Ship24X7.Shipment.Infrastructure.Repositories;

/// <summary>
/// Repository for managing address persistence and retrieval operations.
/// Handles database CRUD operations for sender and receiver addresses in shipment bookings.
/// </summary>
public class AddressRepository : IAddressRepository
{
    private readonly ShipmentDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddressRepository"/> class.
    /// </summary>
    /// <param name="context">Database context for address operations.</param>
    public AddressRepository(ShipmentDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves an address by its unique identifier.
    /// Used for address validation and shipment address lookup.
    /// </summary>
    /// <param name="id">The unique identifier of the address.</param>
    /// <returns>The address entity if found; otherwise, null.</returns>
    public async Task<Address?> GetByIdAsync(Guid id)
    {
        return await _context.Addresses.FindAsync(id);
    }

    /// <summary>
    /// Adds a new address to the database.
    /// Creates address record with contact information and location details.
    /// Used when creating shipments with new sender or receiver addresses.
    /// </summary>
    /// <param name="address">The address entity to add.</param>
    /// <returns>The added address entity with generated ID.</returns>
    public async Task<Address> AddAsync(Address address)
    {
        await _context.Addresses.AddAsync(address);
        await _context.SaveChangesAsync();
        return address;
    }

    /// <summary>
    /// Updates an existing address in the database.
    /// Handles address corrections, contact updates, and geocoding additions.
    /// </summary>
    /// <param name="address">The address entity with updated values.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    public async Task UpdateAsync(Address address)
    {
        _context.Addresses.Update(address);
        await _context.SaveChangesAsync();
    }
}
