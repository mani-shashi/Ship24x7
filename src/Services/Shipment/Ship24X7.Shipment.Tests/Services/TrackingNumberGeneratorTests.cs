using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Ship24X7.Shipment.Domain.Enums;
using Ship24X7.Shipment.Infrastructure.Persistence;
using Ship24X7.Shipment.Infrastructure.Services;
using Xunit;

namespace Ship24X7.Shipment.Tests.Services;

/// <summary>
/// TrackingNumberGeneratorTests implementation. Provides functionality for the application.
/// </summary>
public class TrackingNumberGeneratorTests : IDisposable
{
    private readonly ShipmentDbContext _context;
    private readonly TrackingNumberGenerator _sut;

    public TrackingNumberGeneratorTests()
    {
        var options = new DbContextOptionsBuilder<ShipmentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ShipmentDbContext(options);
        _sut = new TrackingNumberGenerator(_context);
    }

    [Fact]
    public async Task GenerateAsync_WithNoExistingShipments_GeneratesFirstTrackingNumber()
    {
        // Act
        var trackingNumber = await _sut.GenerateAsync();

        // Assert
        var today = DateTime.UtcNow.Date.ToString("yyyyMMdd");
        trackingNumber.Should().StartWith($"SHIP24X7-{today}");
        trackingNumber.Should().EndWith("000001");
        trackingNumber.Should().MatchRegex(@"^SHIP24X7-\d{8}\d{6}$");
    }

    [Fact]
    public async Task GenerateAsync_WithExistingShipments_GeneratesSequentialTrackingNumber()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        
        // Add 3 existing shipments created today
        for (int i = 0; i < 3; i++)
        {
            _context.Shipments.Add(new Domain.Entities.Shipment
            {
                Id = Guid.NewGuid(),
                TrackingNumber = $"SHIP24X7-{today:yyyyMMdd}{(i + 1):D6}",
                CustomerId = Guid.NewGuid(),
                Status = ShipmentStatus.Draft,
                SenderAddressId = Guid.NewGuid(),
                ReceiverAddressId = Guid.NewGuid(),
                ServiceRateId = Guid.NewGuid(),
                CreatedAt = today.AddHours(i),
                CreatedBy = Guid.NewGuid()
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var trackingNumber = await _sut.GenerateAsync();

        // Assert
        var expectedDate = today.ToString("yyyyMMdd");
        trackingNumber.Should().Be($"SHIP24X7-{expectedDate}000004");
    }

    [Fact]
    public async Task GenerateAsync_WithShipmentsFromPreviousDays_StartsNewSequence()
    {
        // Arrange
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        
        // Add shipments from yesterday
        for (int i = 0; i < 5; i++)
        {
            _context.Shipments.Add(new Domain.Entities.Shipment
            {
                Id = Guid.NewGuid(),
                TrackingNumber = $"SHIP24X7-{yesterday:yyyyMMdd}{(i + 1):D6}",
                CustomerId = Guid.NewGuid(),
                Status = ShipmentStatus.Draft,
                SenderAddressId = Guid.NewGuid(),
                ReceiverAddressId = Guid.NewGuid(),
                ServiceRateId = Guid.NewGuid(),
                CreatedAt = yesterday.AddHours(i),
                CreatedBy = Guid.NewGuid()
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var trackingNumber = await _sut.GenerateAsync();

        // Assert
        var today = DateTime.UtcNow.Date.ToString("yyyyMMdd");
        trackingNumber.Should().Be($"SHIP24X7-{today}000001"); // New sequence for today
    }

    [Fact]
    public async Task GenerateAsync_CalledMultipleTimes_GeneratesUniqueTrackingNumbers()
    {
        // Act
        var trackingNumbers = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            var trackingNumber = await _sut.GenerateAsync();
            trackingNumbers.Add(trackingNumber);
            
            // Simulate saving the shipment
            _context.Shipments.Add(new Domain.Entities.Shipment
            {
                Id = Guid.NewGuid(),
                TrackingNumber = trackingNumber,
                CustomerId = Guid.NewGuid(),
                Status = ShipmentStatus.Draft,
                SenderAddressId = Guid.NewGuid(),
                ReceiverAddressId = Guid.NewGuid(),
                ServiceRateId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.NewGuid()
            });
            await _context.SaveChangesAsync();
        }

        // Assert
        trackingNumbers.Should().HaveCount(10);
        trackingNumbers.Should().OnlyHaveUniqueItems();
        
        var today = DateTime.UtcNow.Date.ToString("yyyyMMdd");
        for (int i = 0; i < 10; i++)
        {
            trackingNumbers[i].Should().Be($"SHIP24X7-{today}{(i + 1):D6}");
        }
    }

    [Fact]
    public async Task GenerateAsync_WithConcurrentCalls_GeneratesUniqueTrackingNumbers()
    {
        // Act
        var tasks = Enumerable.Range(0, 20).Select(async i =>
        {
            var trackingNumber = await _sut.GenerateAsync();
            
            // Simulate saving the shipment
            _context.Shipments.Add(new Domain.Entities.Shipment
            {
                Id = Guid.NewGuid(),
                TrackingNumber = trackingNumber,
                CustomerId = Guid.NewGuid(),
                Status = ShipmentStatus.Draft,
                SenderAddressId = Guid.NewGuid(),
                ReceiverAddressId = Guid.NewGuid(),
                ServiceRateId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.NewGuid()
            });
            await _context.SaveChangesAsync();
            
            return trackingNumber;
        });

        var trackingNumbers = await Task.WhenAll(tasks);

        // Assert
        trackingNumbers.Should().HaveCount(20);
        trackingNumbers.Should().OnlyHaveUniqueItems(); // Critical: No duplicates even with concurrent calls
    }

    [Fact]
    public async Task GenerateAsync_FormatMatchesSpecification()
    {
        // Act
        var trackingNumber = await _sut.GenerateAsync();

        // Assert
        // Format: SHIP24X7-{YYYYMMDD}{sequence}
        trackingNumber.Should().MatchRegex(@"^SHIP24X7-\d{8}\d{6}$");
        trackingNumber.Should().HaveLength(22); // SHIP24X7- (9) + YYYYMMDD (8) + sequence (6) = 23
    }

    [Fact]
    public async Task GenerateAsync_SequenceHasSixDigits()
    {
        // Act
        var trackingNumber = await _sut.GenerateAsync();

        // Assert
        var sequencePart = trackingNumber.Substring(17); // After "SHIP24X7-YYYYMMDD"
        sequencePart.Should().HaveLength(6);
        sequencePart.Should().MatchRegex(@"^\d{6}$");
    }

    [Fact]
    public async Task GenerateAsync_WithHighVolume_HandlesLargeSequenceNumbers()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        
        // Add 999 existing shipments
        for (int i = 0; i < 999; i++)
        {
            _context.Shipments.Add(new Domain.Entities.Shipment
            {
                Id = Guid.NewGuid(),
                TrackingNumber = $"SHIP24X7-{today:yyyyMMdd}{(i + 1):D6}",
                CustomerId = Guid.NewGuid(),
                Status = ShipmentStatus.Draft,
                SenderAddressId = Guid.NewGuid(),
                ReceiverAddressId = Guid.NewGuid(),
                ServiceRateId = Guid.NewGuid(),
                CreatedAt = today.AddSeconds(i),
                CreatedBy = Guid.NewGuid()
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var trackingNumber = await _sut.GenerateAsync();

        // Assert
        var expectedDate = today.ToString("yyyyMMdd");
        trackingNumber.Should().Be($"SHIP24X7-{expectedDate}001000");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
