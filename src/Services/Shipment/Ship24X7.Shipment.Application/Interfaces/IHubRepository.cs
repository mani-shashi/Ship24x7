using Ship24X7.Shipment.Domain.Entities;

namespace Ship24X7.Shipment.Application.Interfaces;

/// <summary>
/// Repository for managing IHub persistence operations. Handles database CRUD operations and queries.
/// </summary>
public interface IHubRepository
{
    Task<Hub?> GetByIdAsync(Guid id);
    Task<List<Hub>> GetAllAsync(bool activeOnly = true);
    Task<Hub> AddAsync(Hub hub);
    Task UpdateAsync(Hub hub);
}
