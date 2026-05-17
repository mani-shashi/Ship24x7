using Microsoft.EntityFrameworkCore;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Domain.Entities;
using Ship24X7.Auth.Infrastructure.Persistence;

namespace Ship24X7.Auth.Infrastructure.Repositories;

/// <summary>
/// Repository for managing user preferences persistence.
/// </summary>
public class UserPreferencesRepository : IUserPreferencesRepository
{
    private readonly AuthDbContext _context;

    public UserPreferencesRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<UserPreferences?> GetByUserIdAsync(Guid userId)
    {
        return await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<UserPreferences> AddAsync(UserPreferences preferences)
    {
        await _context.UserPreferences.AddAsync(preferences);
        await _context.SaveChangesAsync();
        return preferences;
    }

    public async Task UpdateAsync(UserPreferences preferences)
    {
        _context.UserPreferences.Update(preferences);
        await _context.SaveChangesAsync();
    }
}
