using Microsoft.EntityFrameworkCore;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Domain.Entities;
using Ship24X7.Auth.Infrastructure.Persistence;

namespace Ship24X7.Auth.Infrastructure.Repositories;

/// <summary>
/// Repository for managing user address book persistence.
/// </summary>
public class UserAddressRepository : IUserAddressRepository
{
    private readonly AuthDbContext _context;

    public UserAddressRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserAddress>> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserAddresses
            .Where(a => a.UserId == userId && !a.IsDeleted)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<UserAddress?> GetByIdAsync(Guid id)
    {
        return await _context.UserAddresses
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
    }

    public async Task<UserAddress> AddAsync(UserAddress address)
    {
        await _context.UserAddresses.AddAsync(address);
        await _context.SaveChangesAsync();
        return address;
    }

    public async Task UpdateAsync(UserAddress address)
    {
        _context.UserAddresses.Update(address);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var address = await _context.UserAddresses.FindAsync(id);
        if (address != null)
        {
            address.IsDeleted = true;
            address.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ClearDefaultAsync(Guid userId)
    {
        var defaults = await _context.UserAddresses
            .Where(a => a.UserId == userId && a.IsDefault && !a.IsDeleted)
            .ToListAsync();

        foreach (var a in defaults)
            a.IsDefault = false;

        await _context.SaveChangesAsync();
    }
}
