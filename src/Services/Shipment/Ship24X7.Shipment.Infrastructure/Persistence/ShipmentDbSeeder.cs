using Microsoft.EntityFrameworkCore;
using Ship24X7.Shipment.Domain.Entities;

namespace Ship24X7.Shipment.Infrastructure.Persistence;

/// <summary>
/// ShipmentDbSeeder implementation. Provides functionality for the application.
/// </summary>
public static class ShipmentDbSeeder
{
    public static async Task SeedAsync(ShipmentDbContext context)
    {
        // Ensure database is created
        await context.Database.MigrateAsync();

        // Seed Service Rates
        if (!await context.ServiceRates.AnyAsync())
        {
            var serviceRates = new List<ServiceRate>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ServiceType = "Domestic",
                    BaseRatePerKg = 50.00m,
                    FuelSurchargePercent = 10.00m,
                    MinimumCharge = 100.00m,
                    EstimatedDeliveryDays = 3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ServiceType = "Express",
                    BaseRatePerKg = 100.00m,
                    FuelSurchargePercent = 12.00m,
                    MinimumCharge = 200.00m,
                    EstimatedDeliveryDays = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ServiceType = "International",
                    BaseRatePerKg = 200.00m,
                    FuelSurchargePercent = 15.00m,
                    MinimumCharge = 500.00m,
                    EstimatedDeliveryDays = 7,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    ServiceType = "Freight",
                    BaseRatePerKg = 30.00m,
                    FuelSurchargePercent = 8.00m,
                    MinimumCharge = 1000.00m,
                    EstimatedDeliveryDays = 5,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.ServiceRates.AddRangeAsync(serviceRates);
            await context.SaveChangesAsync();
        }

        // Seed Sample Hubs
        if (!await context.Hubs.AnyAsync())
        {
            var hubs = new List<Hub>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Mumbai Central Hub",
                    Code = "MUM-01",
                    AddressLine1 = "123 Hub Street",
                    AddressLine2 = "Andheri East",
                    City = "Mumbai",
                    State = "Maharashtra",
                    PostalCode = "400001",
                    Country = "India",
                    Latitude = 19.0760m,
                    Longitude = 72.8777m,
                    Capacity = 10000,
                    CurrentLoad = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Delhi North Hub",
                    Code = "DEL-01",
                    AddressLine1 = "456 Distribution Avenue",
                    AddressLine2 = "Connaught Place",
                    City = "New Delhi",
                    State = "Delhi",
                    PostalCode = "110001",
                    Country = "India",
                    Latitude = 28.7041m,
                    Longitude = 77.1025m,
                    Capacity = 8000,
                    CurrentLoad = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Bangalore Tech Hub",
                    Code = "BLR-01",
                    AddressLine1 = "789 Logistics Park",
                    AddressLine2 = "Whitefield",
                    City = "Bangalore",
                    State = "Karnataka",
                    PostalCode = "560001",
                    Country = "India",
                    Latitude = 12.9716m,
                    Longitude = 77.5946m,
                    Capacity = 12000,
                    CurrentLoad = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Chennai South Hub",
                    Code = "CHE-01",
                    AddressLine1 = "321 Warehouse Road",
                    AddressLine2 = "Guindy",
                    City = "Chennai",
                    State = "Tamil Nadu",
                    PostalCode = "600001",
                    Country = "India",
                    Latitude = 13.0827m,
                    Longitude = 80.2707m,
                    Capacity = 7000,
                    CurrentLoad = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Kolkata East Hub",
                    Code = "KOL-01",
                    AddressLine1 = "654 Shipping Lane",
                    AddressLine2 = "Salt Lake",
                    City = "Kolkata",
                    State = "West Bengal",
                    PostalCode = "700001",
                    Country = "India",
                    Latitude = 22.5726m,
                    Longitude = 88.3639m,
                    Capacity = 6000,
                    CurrentLoad = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await context.Hubs.AddRangeAsync(hubs);
            await context.SaveChangesAsync();
        }
    }
}
