using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Ship24X7.IntegrationTests.Infrastructure;
using Ship24X7.Tracking.Infrastructure.Messaging;
using Ship24X7.Shared.Domain;
using Xunit;

namespace Ship24X7.IntegrationTests.EventFlows;

/// <summary>
/// Integration tests for ShipmentDelivered event flow
/// Tests: Tracking Service -> RabbitMQ -> Notification Service
/// </summary>
public class ShipmentDeliveredEventFlowTests : IClassFixture<RabbitMqTestFixture>
{
    private readonly RabbitMqTestFixture _fixture;

    public ShipmentDeliveredEventFlowTests(RabbitMqTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ShipmentDelivered_Event_Should_Be_Published_To_RabbitMQ()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var shipmentId = Guid.NewGuid();
        var trackingNumber = "SHIP24X7-20240101020";
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        using var publisher = new TrackingEventPublisher(configuration);
        using var consumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.shipment.delivered",
            "shipment.delivered");

        var shipmentDeliveredEvent = new ShipmentDeliveredEvent
        {
            ShipmentId = shipmentId,
            TrackingNumber = trackingNumber,
            DeliveryDate = DateTime.UtcNow,
            ReceivedBy = "John Doe",
            Latitude = 28.6139m,
            Longitude = 77.2090m,
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        };

        // Act
        await publisher.PublishAsync(shipmentDeliveredEvent);

        // Assert
        var receivedEvent = await consumer.WaitForEventAsync("ShipmentDeliveredEvent", TimeSpan.FromSeconds(5));
        
        receivedEvent.Should().NotBeNull();
        receivedEvent!.RoutingKey.Should().Be("shipment.delivered");
        receivedEvent.EventType.Should().Be("ShipmentDeliveredEvent");
        receivedEvent.CorrelationId.Should().Be(correlationId);
        
        var deserializedEvent = JsonSerializer.Deserialize<ShipmentDeliveredEvent>(receivedEvent.Message);
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.ShipmentId.Should().Be(shipmentId);
        deserializedEvent.TrackingNumber.Should().Be(trackingNumber);
        deserializedEvent.ReceivedBy.Should().Be("John Doe");
    }

    [Fact]
    public async Task ShipmentDelivered_Event_Should_Include_GPS_Coordinates()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var latitude = 19.0760m;
        var longitude = 72.8777m;
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        using var publisher = new TrackingEventPublisher(configuration);
        using var consumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.shipment.delivered.gps",
            "shipment.delivered");

        var shipmentDeliveredEvent = new ShipmentDeliveredEvent
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20240101021",
            DeliveryDate = DateTime.UtcNow,
            ReceivedBy = "Jane Smith",
            Latitude = latitude,
            Longitude = longitude,
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        };

        // Act
        await publisher.PublishAsync(shipmentDeliveredEvent);

        // Assert
        var receivedEvent = await consumer.WaitForEventAsync("ShipmentDeliveredEvent", TimeSpan.FromSeconds(5));
        
        receivedEvent.Should().NotBeNull();
        
        var deserializedEvent = JsonSerializer.Deserialize<ShipmentDeliveredEvent>(receivedEvent!.Message);
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.Latitude.Should().Be(latitude);
        deserializedEvent.Longitude.Should().Be(longitude);
    }

    [Fact]
    public async Task ShipmentDelivered_Event_Should_Trigger_Notification_Sending()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        using var publisher = new TrackingEventPublisher(configuration);
        using var notificationConsumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.notification.shipment.delivered",
            "shipment.delivered");

        var shipmentDeliveredEvent = new ShipmentDeliveredEvent
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20240101022",
            DeliveryDate = DateTime.UtcNow,
            ReceivedBy = "Test Recipient",
            Latitude = 12.9716m,
            Longitude = 77.5946m,
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        };

        // Act
        await publisher.PublishAsync(shipmentDeliveredEvent);

        // Assert - Notification service should receive the event
        var receivedEvent = await notificationConsumer.WaitForEventAsync("ShipmentDeliveredEvent", TimeSpan.FromSeconds(5));
        
        receivedEvent.Should().NotBeNull();
        receivedEvent!.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public async Task ShipmentDelivered_Event_Should_Include_Delivery_Timestamp()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var deliveryDate = DateTime.UtcNow;
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        using var publisher = new TrackingEventPublisher(configuration);
        using var consumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.shipment.delivered.timestamp",
            "shipment.delivered");

        var shipmentDeliveredEvent = new ShipmentDeliveredEvent
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20240101023",
            DeliveryDate = deliveryDate,
            ReceivedBy = "Delivery Test",
            Latitude = 22.5726m,
            Longitude = 88.3639m,
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        };

        // Act
        await publisher.PublishAsync(shipmentDeliveredEvent);

        // Assert
        var receivedEvent = await consumer.WaitForEventAsync("ShipmentDeliveredEvent", TimeSpan.FromSeconds(5));
        
        receivedEvent.Should().NotBeNull();
        
        var deserializedEvent = JsonSerializer.Deserialize<ShipmentDeliveredEvent>(receivedEvent!.Message);
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.DeliveryDate.Should().BeCloseTo(deliveryDate, TimeSpan.FromSeconds(1));
    }
}

// Event DTO for testing
public class ShipmentDeliveredEvent : IDomainEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public Guid ShipmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public DateTime DeliveryDate { get; set; }
    public string ReceivedBy { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}
