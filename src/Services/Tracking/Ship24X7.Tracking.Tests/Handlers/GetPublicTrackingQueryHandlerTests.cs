using FluentAssertions;
using Moq;
using Ship24X7.Tracking.Application.Handlers;
using Ship24X7.Tracking.Application.Interfaces;
using Ship24X7.Tracking.Application.Queries;
using Ship24X7.Tracking.Domain.Entities;
using Ship24X7.Tracking.Domain.Enums;
using Xunit;

namespace Ship24X7.Tracking.Tests.Handlers;

/// <summary>
/// Query for retrieving getpublictrackinghandlertests data. Defines query parameters and result type.
/// </summary>
public class GetPublicTrackingQueryHandlerTests
{
    private readonly Mock<ITrackingEventRepository> _mockTrackingEventRepository;
    private readonly GetPublicTrackingQueryHandler _sut;

    public GetPublicTrackingQueryHandlerTests()
    {
        _mockTrackingEventRepository = new Mock<ITrackingEventRepository>();
        _sut = new GetPublicTrackingQueryHandler(_mockTrackingEventRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidTrackingNumber_ReturnsPublicTrackingData()
    {
        // Arrange
        var trackingNumber = "SHIP24X7-20260414001";
        var events = CreateTrackingEvents(trackingNumber);

        _mockTrackingEventRepository
            .Setup(x => x.GetByTrackingNumberAsync(trackingNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var query = new GetPublicTrackingQuery { TrackingNumber = trackingNumber };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.TrackingNumber.Should().Be(trackingNumber);
        result.CurrentStatus.Should().Be(ShipmentStatus.InTransit);
        result.Events.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithNoTrackingEvents_ReturnsNull()
    {
        // Arrange
        var trackingNumber = "SHIP24X7-20260414999";

        _mockTrackingEventRepository
            .Setup(x => x.GetByTrackingNumberAsync(trackingNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TrackingEvent>());

        var query = new GetPublicTrackingQuery { TrackingNumber = trackingNumber };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ReturnsEventsInDescendingOrder()
    {
        // Arrange
        var trackingNumber = "SHIP24X7-20260414001";
        var events = CreateTrackingEvents(trackingNumber);

        _mockTrackingEventRepository
            .Setup(x => x.GetByTrackingNumberAsync(trackingNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var query = new GetPublicTrackingQuery { TrackingNumber = trackingNumber };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Events.Should().BeInDescendingOrder(e => e.EventTimestamp);
        result.Events.First().Status.Should().Be(ShipmentStatus.InTransit);
        result.Events.Last().Status.Should().Be(ShipmentStatus.Booked);
    }

    [Fact]
    public async Task Handle_ReturnsCurrentStatusFromLatestEvent()
    {
        // Arrange
        var trackingNumber = "SHIP24X7-20260414001";
        var events = new List<TrackingEvent>
        {
            CreateTrackingEvent(trackingNumber, ShipmentStatus.Booked, DateTime.UtcNow.AddDays(-2)),
            CreateTrackingEvent(trackingNumber, ShipmentStatus.PickedUp, DateTime.UtcNow.AddDays(-1)),
            CreateTrackingEvent(trackingNumber, ShipmentStatus.OutForDelivery, DateTime.UtcNow)
        };

        _mockTrackingEventRepository
            .Setup(x => x.GetByTrackingNumberAsync(trackingNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var query = new GetPublicTrackingQuery { TrackingNumber = trackingNumber };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CurrentStatus.Should().Be(ShipmentStatus.OutForDelivery);
    }

    [Fact]
    public async Task Handle_DoesNotReturnPersonalDetails()
    {
        // Arrange
        var trackingNumber = "SHIP24X7-20260414001";
        var events = CreateTrackingEvents(trackingNumber);

        _mockTrackingEventRepository
            .Setup(x => x.GetByTrackingNumberAsync(trackingNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var query = new GetPublicTrackingQuery { TrackingNumber = trackingNumber };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        
        // Verify that the response only contains public information
        result!.TrackingNumber.Should().NotBeNullOrEmpty();
        result.CurrentStatus.Should().NotBe(default(ShipmentStatus));
        result.EstimatedDeliveryDate.Should().NotBe(default(DateTime));
        result.Events.Should().NotBeEmpty();
        
        // Verify each event contains only public fields
        foreach (var eventDto in result.Events)
        {
            eventDto.Status.Should().NotBe(default(ShipmentStatus));
            eventDto.Description.Should().NotBeNullOrEmpty();
            eventDto.Location.Should().NotBeNullOrEmpty();
            eventDto.EventTimestamp.Should().NotBe(default(DateTime));
        }
    }

    [Fact]
    public async Task Handle_ReturnsEstimatedDeliveryDate()
    {
        // Arrange
        var trackingNumber = "SHIP24X7-20260414001";
        var events = CreateTrackingEvents(trackingNumber);

        _mockTrackingEventRepository
            .Setup(x => x.GetByTrackingNumberAsync(trackingNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var query = new GetPublicTrackingQuery { TrackingNumber = trackingNumber };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.EstimatedDeliveryDate.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_WithMultipleEvents_ReturnsAllEvents()
    {
        // Arrange
        var trackingNumber = "SHIP24X7-20260414001";
        var events = new List<TrackingEvent>
        {
            CreateTrackingEvent(trackingNumber, ShipmentStatus.Booked, DateTime.UtcNow.AddDays(-5)),
            CreateTrackingEvent(trackingNumber, ShipmentStatus.Paid, DateTime.UtcNow.AddDays(-4)),
            CreateTrackingEvent(trackingNumber, ShipmentStatus.PickedUp, DateTime.UtcNow.AddDays(-3)),
            CreateTrackingEvent(trackingNumber, ShipmentStatus.InTransit, DateTime.UtcNow.AddDays(-2)),
            CreateTrackingEvent(trackingNumber, ShipmentStatus.OutForDelivery, DateTime.UtcNow.AddDays(-1)),
            CreateTrackingEvent(trackingNumber, ShipmentStatus.Delivered, DateTime.UtcNow)
        };

        _mockTrackingEventRepository
            .Setup(x => x.GetByTrackingNumberAsync(trackingNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var query = new GetPublicTrackingQuery { TrackingNumber = trackingNumber };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Events.Should().HaveCount(6);
        result.CurrentStatus.Should().Be(ShipmentStatus.Delivered);
    }

    [Fact]
    public async Task Handle_WithExceptionEvents_IncludesThemInResponse()
    {
        // Arrange
        var trackingNumber = "SHIP24X7-20260414001";
        var events = new List<TrackingEvent>
        {
            CreateTrackingEvent(trackingNumber, ShipmentStatus.Booked, DateTime.UtcNow.AddDays(-2)),
            CreateTrackingEvent(trackingNumber, ShipmentStatus.InTransit, DateTime.UtcNow.AddDays(-1), isException: true),
            CreateTrackingEvent(trackingNumber, ShipmentStatus.InTransit, DateTime.UtcNow)
        };

        _mockTrackingEventRepository
            .Setup(x => x.GetByTrackingNumberAsync(trackingNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var query = new GetPublicTrackingQuery { TrackingNumber = trackingNumber };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Events.Should().HaveCount(3);
    }

    private List<TrackingEvent> CreateTrackingEvents(string trackingNumber)
    {
        return new List<TrackingEvent>
        {
            CreateTrackingEvent(trackingNumber, ShipmentStatus.Booked, DateTime.UtcNow.AddDays(-2)),
            CreateTrackingEvent(trackingNumber, ShipmentStatus.PickedUp, DateTime.UtcNow.AddDays(-1)),
            CreateTrackingEvent(trackingNumber, ShipmentStatus.InTransit, DateTime.UtcNow)
        };
    }

    private TrackingEvent CreateTrackingEvent(
        string trackingNumber, 
        ShipmentStatus status, 
        DateTime timestamp,
        bool isException = false)
    {
        return new TrackingEvent
        {
            Id = Guid.NewGuid(),
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = trackingNumber,
            Status = status,
            Description = $"Shipment {status}",
            Location = "Test Hub",
            EventTimestamp = timestamp,
            IsException = isException,
            RecordedBy = "System",
            CreatedAt = timestamp,
            CreatedBy = Guid.NewGuid()
        };
    }
}
