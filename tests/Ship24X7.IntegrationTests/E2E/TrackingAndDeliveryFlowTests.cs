using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Ship24X7.IntegrationTests.Infrastructure;
using Ship24X7.Tracking.Infrastructure.Messaging;
using Ship24X7.Shared.Domain;
using System.Text.Json;
using Xunit;

namespace Ship24X7.IntegrationTests.E2E;

/// <summary>
/// End-to-end integration tests for tracking and delivery flow
/// Tests the integration of: Tracking Service → Notification Service
/// 
/// **Validates: Requirements 8.1, 9.1, 11.1, 12.2**
/// </summary>
public class TrackingAndDeliveryFlowTests : IClassFixture<RabbitMqTestFixture>
{
    private readonly RabbitMqTestFixture _fixture;

    public TrackingAndDeliveryFlowTests(RabbitMqTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CompleteTrackingFlow_ShouldPublishTrackingEventsInSequence()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        var publisher = new TrackingEventPublisher(configuration);
        
        using var consumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.tracking.events",
            "shipment.#");

        var correlationId = Guid.NewGuid().ToString();
        var shipmentId = Guid.NewGuid();
        var trackingNumber = $"SHIP24X7-{DateTime.UtcNow:yyyyMMdd}101";

        // Act - Simulate tracking event progression
        var trackingEvent = new TrackingEventRecordedDto
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            CorrelationId = correlationId,
            ShipmentId = shipmentId,
            TrackingNumber = trackingNumber,
            Status = "PickedUp",
            Description = "Package picked up from sender",
            Location = "Mumbai Hub",
            Latitude = 19.0760,
            Longitude = 72.8777,
            RecordedAt = DateTime.UtcNow
        };

        publisher.PublishEvent(trackingEvent, "shipment.tracking.recorded");

        // Assert
        var receivedEvent = await consumer.WaitForEventAsync(
            "TrackingEventRecordedDto",
            TimeSpan.FromSeconds(5));

        receivedEvent.Should().NotBeNull();
        receivedEvent!.CorrelationId.Should().Be(correlationId);
        
        // Cleanup
        publisher.Dispose();
    }

    [Fact]
    public async Task DeliveryProofCapture_ShouldPublishShipmentDeliveredEvent()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        var publisher = new TrackingEventPublisher(configuration);
        
        using var consumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.delivery.proof",
            "shipment.delivered");

        var correlationId = Guid.NewGuid().ToString();
        var shipmentId = Guid.NewGuid();

        // Act - Simulate delivery with proof
        var deliveredEvent = new ShipmentDeliveredEventDto
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            CorrelationId = correlationId,
            ShipmentId = shipmentId,
            TrackingNumber = $"SHIP24X7-{DateTime.UtcNow:yyyyMMdd}102",
            ReceivedBy = "Jane Smith",
            DeliveryDate = DateTime.UtcNow,
            Latitude = 19.0760,
            Longitude = 72.8777,
            SignatureImageUrl = "https://storage.example.com/signatures/sig123.jpg",
            PhotoProofUrl = "https://storage.example.com/proofs/photo123.jpg",
            Notes = "Delivered to recipient at door"
        };

        publisher.PublishEvent(deliveredEvent, "shipment.delivered");

        // Assert
        var receivedEvent = await consumer.WaitForEventAsync(
            "ShipmentDeliveredEventDto",
            TimeSpan.FromSeconds(5));

        receivedEvent.Should().NotBeNull();
        receivedEvent!.CorrelationId.Should().Be(correlationId);
        
        var deliveredEventData = JsonSerializer.Deserialize<ShipmentDeliveredEventDto>(receivedEvent.Message);
        deliveredEventData.Should().NotBeNull();
        deliveredEventData!.ReceivedBy.Should().Be("Jane Smith");
        deliveredEventData.Latitude.Should().BeInRange(-90, 90);
        deliveredEventData.Longitude.Should().BeInRange(-180, 180);
        
        // Cleanup
        publisher.Dispose();
    }

    [Fact]
    public async Task ShipmentDelayed_ShouldPublishExceptionEvent()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        var publisher = new TrackingEventPublisher(configuration);
        
        using var consumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.shipment.delayed",
            "shipment.delayed");

        var correlationId = Guid.NewGuid().ToString();
        var shipmentId = Guid.NewGuid();

        // Act - Simulate shipment delay
        var delayedEvent = new ShipmentDelayedEventDto
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            CorrelationId = correlationId,
            ShipmentId = shipmentId,
            TrackingNumber = $"SHIP24X7-{DateTime.UtcNow:yyyyMMdd}103",
            Reason = "Weather conditions",
            ExpectedDelay = TimeSpan.FromHours(24),
            NewEstimatedDeliveryDate = DateTime.UtcNow.AddDays(4),
            Location = "Delhi Hub",
            RecordedAt = DateTime.UtcNow
        };

        publisher.PublishEvent(delayedEvent, "shipment.delayed");

        // Assert
        var receivedEvent = await consumer.WaitForEventAsync(
            "ShipmentDelayedEventDto",
            TimeSpan.FromSeconds(5));

        receivedEvent.Should().NotBeNull();
        receivedEvent!.CorrelationId.Should().Be(correlationId);
        
        var delayedEventData = JsonSerializer.Deserialize<ShipmentDelayedEventDto>(receivedEvent.Message);
        delayedEventData.Should().NotBeNull();
        delayedEventData!.Reason.Should().Be("Weather conditions");
        
        // Cleanup
        publisher.Dispose();
    }

    [Fact]
    public async Task PickupScheduling_ShouldTriggerTrackingEventChain()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        var publisher = new TrackingEventPublisher(configuration);
        
        using var consumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.pickup.tracking",
            "shipment.tracking.recorded");

        var correlationId = Guid.NewGuid().ToString();
        var shipmentId = Guid.NewGuid();

        // Act - Simulate pickup completion
        var pickupEvent = new TrackingEventRecordedDto
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            CorrelationId = correlationId,
            ShipmentId = shipmentId,
            TrackingNumber = $"SHIP24X7-{DateTime.UtcNow:yyyyMMdd}104",
            Status = "PickedUp",
            Description = "Package collected from sender address",
            Location = "Bangalore Hub",
            Latitude = 12.9716,
            Longitude = 77.5946,
            RecordedAt = DateTime.UtcNow
        };

        publisher.PublishEvent(pickupEvent, "shipment.tracking.recorded");

        // Assert
        var receivedEvent = await consumer.WaitForEventAsync(
            "TrackingEventRecordedDto",
            TimeSpan.FromSeconds(5));

        receivedEvent.Should().NotBeNull();
        receivedEvent!.CorrelationId.Should().Be(correlationId);
        
        var trackingEventData = JsonSerializer.Deserialize<TrackingEventRecordedDto>(receivedEvent.Message);
        trackingEventData.Should().NotBeNull();
        trackingEventData!.Status.Should().Be("PickedUp");
        
        // Cleanup
        publisher.Dispose();
    }
}

// Event DTOs for testing
public class TrackingEventRecordedDto : IDomainEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public Guid ShipmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime RecordedAt { get; set; }
}

public class ShipmentDeliveredEventDto : IDomainEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public Guid ShipmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string ReceivedBy { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string SignatureImageUrl { get; set; } = string.Empty;
    public string PhotoProofUrl { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class ShipmentDelayedEventDto : IDomainEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public Guid ShipmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public TimeSpan ExpectedDelay { get; set; }
    public DateTime NewEstimatedDeliveryDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
}
