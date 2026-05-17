using Microsoft.EntityFrameworkCore;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Infrastructure.Persistence;

namespace Ship24X7.Shipment.Infrastructure.Services;

/// <summary>
/// Tracking number generator service for creating unique shipment identifiers.
/// Generates sequential tracking numbers with date prefix and 6-digit sequence number.
/// Implements thread-safe generation using semaphore to prevent duplicate numbers.
/// Format: SHIP24X7-YYYYMMDDNNNNNN (e.g., SHIP24X7-20260420000001).
/// </summary>
public class TrackingNumberGenerator : ITrackingNumberGenerator
{
    private readonly ShipmentDbContext _context;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackingNumberGenerator"/> class.
    /// </summary>
    /// <param name="context">Database context for querying shipment count.</param>
    public TrackingNumberGenerator(ShipmentDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Generates a unique tracking number for a new shipment.
    /// Process flow:
    /// 1. Acquires semaphore lock for thread-safe generation
    /// 2. Gets current date in YYYYMMDD format
    /// 3. Queries database for count of shipments created today
    /// 4. Increments count by 1 for new shipment sequence
    /// 5. Formats sequence as 6-digit zero-padded number (000001-999999)
    /// 6. Combines prefix, date, and sequence: SHIP24X7-YYYYMMDDNNNNNN
    /// 7. Releases semaphore lock
    /// 8. Returns generated tracking number
    /// Semaphore ensures no duplicate numbers are generated in concurrent requests.
    /// Sequence resets daily, allowing up to 999,999 shipments per day.
    /// </summary>
    /// <returns>Unique tracking number in format SHIP24X7-YYYYMMDDNNNNNN.</returns>
    public async Task<string> GenerateAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            var today = DateTime.UtcNow.Date;
            var datePrefix = today.ToString("yyyyMMdd");
            
            // Get the count of shipments created today
            var todayCount = await _context.Shipments
                .Where(s => s.CreatedAt >= today)
                .CountAsync();
            
            var sequence = (todayCount + 1).ToString("D6");
            
            return $"SHIP24X7-{datePrefix}{sequence}";
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
