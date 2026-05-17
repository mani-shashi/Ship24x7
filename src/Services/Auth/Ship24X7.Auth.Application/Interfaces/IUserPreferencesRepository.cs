using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Application.Interfaces;

/// <summary>
/// Repository interface for user preferences operations.
/// </summary>
public interface IUserPreferencesRepository
{
    Task<UserPreferences?> GetByUserIdAsync(Guid userId);
    Task<UserPreferences> AddAsync(UserPreferences preferences);
    Task UpdateAsync(UserPreferences preferences);
}
