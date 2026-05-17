using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Application.Interfaces;

/// <summary>
/// Repository interface for user address book operations.
/// </summary>
public interface IUserAddressRepository
{
    Task<IEnumerable<UserAddress>> GetByUserIdAsync(Guid userId);
    Task<UserAddress?> GetByIdAsync(Guid id);
    Task<UserAddress> AddAsync(UserAddress address);
    Task UpdateAsync(UserAddress address);
    Task DeleteAsync(Guid id);
    Task ClearDefaultAsync(Guid userId);
}
