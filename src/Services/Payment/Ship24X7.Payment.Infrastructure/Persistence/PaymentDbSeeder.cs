using Microsoft.EntityFrameworkCore;

namespace Ship24X7.Payment.Infrastructure.Persistence;

/// <summary>
/// PaymentDbSeeder implementation. Provides functionality for the application.
/// </summary>
public static class PaymentDbSeeder
{
    public static async Task SeedAsync(PaymentDbContext context)
    {
        // Ensure database is created
        await context.Database.MigrateAsync();

        // No seed data required for Payment Service
        // Payment orders and refunds are created dynamically through Razorpay integration
    }
}
