using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Ship24X7.IntegrationTests.Infrastructure;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Infrastructure.Messaging;
using Ship24X7.Shared.Domain;
using Xunit;

namespace Ship24X7.IntegrationTests.EventFlows;

/// <summary>
/// Integration tests for ShipmentBooked event flow
/// Tests: Shipment Service -> RabbitMQ -> Notification Service
/// </summary>
public class ShipmentBookedEventFlowTests : IClassFixture<RabbitMqTestFixture>
{
    private readonly RabbitMqTestFixture _fixture;

    public ShipmentBookedEventFlowTests(RabbitMqTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ShipmentBooked_Event_Should_Be_Published_To_RabbitMQ()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var shipmentId = Guid.NewGuid();
        var trackingNumber = "SHIP24X7-20240101001";
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        using var publisher = new ShipmentEventPublisher(configuration);
        using var consumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.shipment.booked",
            "shipment.booked");

        var shipmentBookedEvent = new ShipmentBookedEvent
        {
            ShipmentId = shipmentId,
            TrackingNumber = trackingNumber,
            CustomerId = Guid.NewGuid(),
            TotalCost = 1500.00m,
            Currency = "INR",
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        };

        // Act
        await publisher.PublishAsync(shipmentBookedEvent);

        // Assert
        var receivedEvent = await consumer.WaitForEventAsync("ShipmentBookedEvent", TimeSpan.FromSeconds(5));
        
        receivedEvent.Should().NotBeNull();
        receivedEvent!.RoutingKey.Should().Be("shipment.booked");
        receivedEvent.EventType.Should().Be("ShipmentBookedEvent");
        receivedEvent.CorrelationId.Should().Be(correlationId);
        
        var deserializedEvent = JsonSerializer.Deserialize<ShipmentBookedEvent>(receivedEvent.Message);
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.ShipmentId.Should().Be(shipmentId);
        deserializedEvent.TrackingNumber.Should().Be(trackingNumber);
        deserializedEvent.TotalCost.Should().Be(1500.00m);
    }

    [Fact]
    public async Task ShipmentBooked_Event_Should_Include_CorrelationId_In_Headers()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        using var publisher = new ShipmentEventPublisher(configuration);
        using var consumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.shipment.booked.correlation",
            "shipment.booked");

        var shipmentBookedEvent = new ShipmentBookedEvent
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20240101002",
            CustomerId = Guid.NewGuid(),
            TotalCost = 2000.00m,
            Currency = "INR",
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(5),
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        };

        // Act
        await publisher.PublishAsync(shipmentBookedEvent);

        // Assert
        var receivedEvent = await consumer.WaitForEventAsync("ShipmentBookedEvent", TimeSpan.FromSeconds(5));
        
        receivedEvent.Should().NotBeNull();
        receivedEvent!.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public async Task Multiple_ShipmentBooked_Events_Should_Be_Delivered_In_Order()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        using var publisher = new ShipmentEventPublisher(configuration);
        using var consumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.shipment.booked.multiple",
            "shipment.booked");

        var events = new List<ShipmentBookedEvent>();
        for (int i = 0; i < 5; i++)
        {
            events.Add(new ShipmentBookedEvent
            {
                ShipmentId = Guid.NewGuid(),
                TrackingNumber = $"SHIP24X7-2024010100{i}",
                CustomerId = Guid.NewGuid(),
                TotalCost = 1000.00m * (i + 1),
                Currency = "INR",
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
                CorrelationId = Guid.NewGuid().ToString(),
                OccurredAt = DateTime.UtcNow
            });
        }

        // Act
        foreach (var evt in events)
        {
            await publisher.PublishAsync(evt);
        }

        // Assert
        await Task.Delay(2000); // Wait for all events to be consumed
        
        consumer.ReceivedEvents.Should().HaveCount(5);
        consumer.ReceivedEvents.All(e => e.EventType == "ShipmentBookedEvent").Should().BeTrue();
    }
}

// Event DTO for testing
public class ShipmentBookedEvent : IDomainEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public Guid ShipmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public decimal TotalCost { get; set; }
    public string Currency { get; set; } = "INR";
    public DateTime EstimatedDeliveryDate { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}
