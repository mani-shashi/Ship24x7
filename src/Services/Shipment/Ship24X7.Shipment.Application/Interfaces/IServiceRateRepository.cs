using Ship24X7.Shipment.Domain.Entities;

namespace Ship24X7.Shipment.Application.Interfaces;

/// <summary>
/// Repository for managing IServiceRate persistence operations. Handles database CRUD operations and queries.
/// </summary>
public interface IServiceRateRepository
{
    Task<ServiceRate?> GetByIdAsync(Guid id);
    Task<List<ServiceRate>> GetActiveRatesAsync(string? serviceType = null);
    Task<ServiceRate> AddAsync(ServiceRate serviceRate);
    Task UpdateAsync(ServiceRate serviceRate);
}
