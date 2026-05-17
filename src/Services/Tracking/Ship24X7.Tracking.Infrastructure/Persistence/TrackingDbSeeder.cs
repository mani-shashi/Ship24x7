using Microsoft.EntityFrameworkCore;

namespace Ship24X7.Tracking.Infrastructure.Persistence;

/// <summary>
/// TrackingDbSeeder implementation. Provides functionality for the application.
/// </summary>
public static class TrackingDbSeeder
{
    public static async Task SeedAsync(TrackingDbContext context)
    {
        // Ensure database is created
        await context.Database.MigrateAsync();

        // No seed data required for Tracking Service
        // Tracking events, delivery proofs, and documents are created dynamically
    }
}
