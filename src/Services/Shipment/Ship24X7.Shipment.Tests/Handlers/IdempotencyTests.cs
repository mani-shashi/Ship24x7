using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Ship24X7.Shipment.Application.Commands;
using Ship24X7.Shipment.Application.Handlers;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Domain.Aggregates;
using Ship24X7.Shipment.Domain.Entities;
using Ship24X7.Shipment.Domain.Enums;
using Ship24X7.Shipment.Infrastructure.Persistence;
using Ship24X7.Shipment.Infrastructure.Repositories;
using Ship24X7.Shipment.Infrastructure.Services;
using Xunit;

namespace Ship24X7.Shipment.Tests.Handlers;

/// <summary>
/// IdempotencyTests implementation. Provides functionality for the application.
/// </summary>
public class IdempotencyTests : IDisposable
{
    private readonly ShipmentDbContext _context;
    private readonly Mock<IServiceRateRepository> _mockServiceRateRepository;
    private readonly Mock<IAddressRepository> _mockAddressRepository;
    private readonly CreateShipmentCommandHandler _handler;

    public IdempotencyTests()
    {
        var options = new DbContextOptionsBuilder<ShipmentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ShipmentDbContext(options);
        
        var shipmentRepository = new ShipmentRepository(_context);
        var trackingNumberGenerator = new TrackingNumberGenerator(_context);
        
        _mockServiceRateRepository = new Mock<IServiceRateRepository>();
        _mockAddressRepository = new Mock<IAddressRepository>();
        
        var mockRateCalculationService = new Mock<IRateCalculationService>();
        
        _handler = new CreateShipmentCommandHandler(
            shipmentRepository,
            _mockAddressRepository.Object,
            _mockServiceRateRepository.Object,
            trackingNumberGenerator,
            mockRateCalculationService.Object
        );
    }

    [Fact]
    public async Task CreateShipment_WithUniqueIdempotencyKey_CreatesNewShipment()
    {
        // Arrange
        var serviceRate = CreateServiceRate();
        _mockServiceRateRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(serviceRate);

        _mockAddressRepository
            .Setup(x => x.AddAsync(It.IsAny<Address>()))
            .ReturnsAsync((Address a) => a);

        var command = CreateShipmentCommand("idempotency-key-001");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TrackingNumber.Should().NotBeNullOrEmpty();
        result.Status.Should().Be(ShipmentStatus.Draft);
    }

    [Fact]
    public async Task CreateShipment_WithDuplicateIdempotencyKey_ReturnsSameShipment()
    {
        // Arrange
        var serviceRate = CreateServiceRate();
        _mockServiceRateRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(serviceRate);

        _mockAddressRepository
            .Setup(x => x.AddAsync(It.IsAny<Address>()))
            .ReturnsAsync((Address a) => a);

        var idempotencyKey = "idempotency-key-duplicate-test";
        var command1 = CreateShipmentCommand(idempotencyKey);
        var command2 = CreateShipmentCommand(idempotencyKey);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.Id.Should().Be(result2.Id);
        result1.TrackingNumber.Should().Be(result2.TrackingNumber);
        
        // Verify only one shipment was created
        var shipmentsInDb = await _context.Shipments
            .Where(s => s.IdempotencyKey == idempotencyKey)
            .ToListAsync();
        shipmentsInDb.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateShipment_WithDifferentIdempotencyKeys_CreatesDifferentShipments()
    {
        // Arrange
        var serviceRate = CreateServiceRate();
        _mockServiceRateRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(serviceRate);

        _mockAddressRepository
            .Setup(x => x.AddAsync(It.IsAny<Address>()))
            .ReturnsAsync((Address a) => a);

        var command1 = CreateShipmentCommand("idempotency-key-001");
        var command2 = CreateShipmentCommand("idempotency-key-002");
        var command3 = CreateShipmentCommand("idempotency-key-003");

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);
        var result3 = await _handler.Handle(command3, CancellationToken.None);

        // Assert
        result1.Id.Should().NotBe(result2.Id);
        result2.Id.Should().NotBe(result3.Id);
        result1.Id.Should().NotBe(result3.Id);
        
        result1.TrackingNumber.Should().NotBe(result2.TrackingNumber);
        result2.TrackingNumber.Should().NotBe(result3.TrackingNumber);
        result1.TrackingNumber.Should().NotBe(result3.TrackingNumber);
    }

    [Fact]
    public async Task CreateShipment_WithEmptyIdempotencyKey_CreatesNewShipmentEachTime()
    {
        // Arrange
        var serviceRate = CreateServiceRate();
        _mockServiceRateRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(serviceRate);

        _mockAddressRepository
            .Setup(x => x.AddAsync(It.IsAny<Address>()))
            .ReturnsAsync((Address a) => a);

        var command1 = CreateShipmentCommand(string.Empty);
        var command2 = CreateShipmentCommand(string.Empty);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.Id.Should().NotBe(result2.Id);
        result1.TrackingNumber.Should().NotBe(result2.TrackingNumber);
    }

    [Fact]
    public async Task CreateShipment_WithSameIdempotencyKeyButDifferentData_ReturnsSameOriginalShipment()
    {
        // Arrange
        var serviceRate = CreateServiceRate();
        _mockServiceRateRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(serviceRate);

        _mockAddressRepository
            .Setup(x => x.AddAsync(It.IsAny<Address>()))
            .ReturnsAsync((Address a) => a);

        var idempotencyKey = "idempotency-key-same-key-different-data";
        var command1 = CreateShipmentCommand(idempotencyKey);
        command1.IsFragile = false;
        command1.DeclaredValue = 1000m;

        var command2 = CreateShipmentCommand(idempotencyKey);
        command2.IsFragile = true; // Different data
        command2.DeclaredValue = 5000m; // Different data

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.Id.Should().Be(result2.Id);
        result1.TrackingNumber.Should().Be(result2.TrackingNumber);
        
        // Verify the original shipment data is preserved
        var shipmentInDb = await _context.Shipments.FindAsync(result1.Id);
        shipmentInDb.Should().NotBeNull();
        shipmentInDb!.IsFragile.Should().BeFalse(); // Original value
        shipmentInDb.DeclaredValue.Should().Be(1000m); // Original value
    }

    [Fact]
    public async Task CreateShipment_WithIdempotencyKey_Within24Hours_ReturnsSameShipment()
    {
        // Arrange
        var serviceRate = CreateServiceRate();
        _mockServiceRateRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(serviceRate);

        _mockAddressRepository
            .Setup(x => x.AddAsync(It.IsAny<Address>()))
            .ReturnsAsync((Address a) => a);

        var idempotencyKey = "idempotency-key-24-hour-test";
        var command = CreateShipmentCommand(idempotencyKey);

        // Act - Create shipment
        var result1 = await _handler.Handle(command, CancellationToken.None);
        
        // Simulate time passing (but within 24 hours)
        await Task.Delay(100);
        
        // Try to create again with same key
        var result2 = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result1.Id.Should().Be(result2.Id);
        result1.TrackingNumber.Should().Be(result2.TrackingNumber);
    }

    [Fact]
    public async Task CreateShipment_ConcurrentRequestsWithSameIdempotencyKey_CreatesOnlyOneShipment()
    {
        // Arrange
        var serviceRate = CreateServiceRate();
        _mockServiceRateRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(serviceRate);

        _mockAddressRepository
            .Setup(x => x.AddAsync(It.IsAny<Address>()))
            .ReturnsAsync((Address a) => a);

        var idempotencyKey = "idempotency-key-concurrent-test";
        var commands = Enumerable.Range(0, 10)
            .Select(_ => CreateShipmentCommand(idempotencyKey))
            .ToList();

        // Act - Simulate concurrent requests
        var tasks = commands.Select(cmd => _handler.Handle(cmd, CancellationToken.None));
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10);
        results.Select(r => r.Id).Should().OnlyHaveUniqueItems(
            because: "all concurrent requests with same idempotency key should return the same shipment");
        results.Select(r => r.Id).Distinct().Should().HaveCount(1);
        
        // Verify only one shipment was created in database
        var shipmentsInDb = await _context.Shipments
            .Where(s => s.IdempotencyKey == idempotencyKey)
            .ToListAsync();
        shipmentsInDb.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("key-001")]
    [InlineData("key-002")]
    [InlineData("key-003")]
    [InlineData("UPPERCASE-KEY")]
    [InlineData("lowercase-key")]
    [InlineData("MixedCase-Key-123")]
    public async Task CreateShipment_WithVariousIdempotencyKeyFormats_HandlesCorrectly(string idempotencyKey)
    {
        // Arrange
        var serviceRate = CreateServiceRate();
        _mockServiceRateRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(serviceRate);

        _mockAddressRepository
            .Setup(x => x.AddAsync(It.IsAny<Address>()))
            .ReturnsAsync((Address a) => a);

        var command = CreateShipmentCommand(idempotencyKey);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        
        var shipmentInDb = await _context.Shipments
            .FirstOrDefaultAsync(s => s.IdempotencyKey == idempotencyKey);
        shipmentInDb.Should().NotBeNull();
        shipmentInDb!.IdempotencyKey.Should().Be(idempotencyKey);
    }

    private CreateShipmentCommand CreateShipmentCommand(string idempotencyKey)
    {
        return new CreateShipmentCommand
        {
            CustomerId = Guid.NewGuid(),
            ServiceRateId = Guid.NewGuid(),
            IdempotencyKey = idempotencyKey,
            
            SenderContactName = "John Doe",
            SenderContactPhone = "+919876543210",
            SenderContactEmail = "john@example.com",
            SenderAddressLine1 = "123 Main St",
            SenderAddressLine2 = "Apt 4B",
            SenderCity = "Mumbai",
            SenderState = "Maharashtra",
            SenderPostalCode = "400001",
            SenderCountry = "India",
            
            ReceiverContactName = "Jane Smith",
            ReceiverContactPhone = "+919876543211",
            ReceiverContactEmail = "jane@example.com",
            ReceiverAddressLine1 = "456 Oak Ave",
            ReceiverAddressLine2 = "",
            ReceiverCity = "Delhi",
            ReceiverState = "Delhi",
            ReceiverPostalCode = "110001",
            ReceiverCountry = "India",
            
            Items = new List<ShipmentItemDto>
            {
                new()
                {
                    Description = "Test Package",
                    Quantity = 1,
                    Weight = 5m,
                    Length = 30m,
                    Width = 20m,
                    Height = 10m,
                    PackageType = "Box"
                }
            },
            
            IsFragile = false,
            RequiresRefrigeration = false,
            DeclaredValue = null
        };
    }

    private ServiceRate CreateServiceRate()
    {
        return new ServiceRate
        {
            Id = Guid.NewGuid(),
            ServiceType = "Domestic",
            ServiceName = "Domestic Standard",
            BaseRatePerKg = 10m,
            FuelSurchargePercent = 5m,
            MinimumCharge = 100m,
            EstimatedDeliveryDays = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
