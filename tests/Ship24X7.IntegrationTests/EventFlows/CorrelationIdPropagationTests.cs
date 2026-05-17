using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Ship24X7.IntegrationTests.Infrastructure;
using Ship24X7.Shipment.Infrastructure.Messaging;
using Ship24X7.Payment.Infrastructure.Messaging;
using Ship24X7.Tracking.Infrastructure.Messaging;
using Ship24X7.Shared.Domain;
using Xunit;

namespace Ship24X7.IntegrationTests.EventFlows;

/// <summary>
/// Integration tests for CorrelationId propagation across all event flows
/// Validates Requirement 16.3: CorrelationId must be propagated across all service calls
/// </summary>
public class CorrelationIdPropagationTests : IClassFixture<RabbitMqTestFixture>
{
    private readonly RabbitMqTestFixture _fixture;

    public CorrelationIdPropagationTests(RabbitMqTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task All_Events_Should_Include_CorrelationId_In_Headers()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        using var shipmentPublisher = new ShipmentEventPublisher(configuration);
        using var paymentPublisher = new PaymentEventPublisher(configuration);
        using var trackingPublisher = new TrackingEventPublisher(configuration);
        
        using var consumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.correlation.all",
            "shipment.booked",
            "payment.captured",
            "shipment.delivered");

        // Act - Publish events from different services with same CorrelationId
        await shipmentPublisher.PublishAsync(new ShipmentBookedEventDto
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20240101040",
            CustomerId = Guid.NewGuid(),
            TotalCost = 1500.00m,
            Currency = "INR",
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        });

        await paymentPublisher.PublishAsync(new PaymentCapturedEventDto
        {
            PaymentOrderId = Guid.NewGuid(),
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20240101040",
            RazorpayPaymentId = "pay_test",
            Amount = 1500.00m,
            Currency = "INR",
            CapturedAt = DateTime.UtcNow,
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        });

        await trackingPublisher.PublishAsync(new ShipmentDeliveredEventDto
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20240101040",
            DeliveryDate = DateTime.UtcNow,
            ReceivedBy = "Test User",
            Latitude = 28.6139m,
            Longitude = 77.2090m,
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        });

        // Wait for all events
        await Task.Delay(2000);

        // Assert
        consumer.ReceivedEvents.Should().HaveCount(3);
        consumer.ReceivedEvents.All(e => e.CorrelationId == correlationId)
            .Should().BeTrue("All events should have the same CorrelationId");
    }

    [Fact]
    public async Task CorrelationId_Should_Be_Unique_Per_Request()
    {
        // Arrange
        var correlationId1 = Guid.NewGuid().ToString();
        var correlationId2 = Guid.NewGuid().ToString();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        using var publisher = new ShipmentEventPublisher(configuration);
        using var consumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.correlation.unique",
            "shipment.booked");

        // Act - Publish two events with different CorrelationIds
        await publisher.PublishAsync(new ShipmentBookedEventDto
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20240101041",
            CustomerId = Guid.NewGuid(),
            TotalCost = 1500.00m,
            Currency = "INR",
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
            CorrelationId = correlationId1,
            OccurredAt = DateTime.UtcNow
        });

        await publisher.PublishAsync(new ShipmentBookedEventDto
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20240101042",
            CustomerId = Guid.NewGuid(),
            TotalCost = 2000.00m,
            Currency = "INR",
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
            CorrelationId = correlationId2,
            OccurredAt = DateTime.UtcNow
        });

        await Task.Delay(1000);

        // Assert
        consumer.ReceivedEvents.Should().HaveCount(2);
        consumer.ReceivedEvents.Select(e => e.CorrelationId).Distinct().Should().HaveCount(2);
        consumer.ReceivedEvents.Should().Contain(e => e.CorrelationId == correlationId1);
        consumer.ReceivedEvents.Should().Contain(e => e.CorrelationId == correlationId2);
    }

    [Fact]
    public async Task CorrelationId_Should_Be_Traceable_Across_Event_Chain()
    {
        // Arrange - Simulate a complete flow: Shipment -> Payment -> Delivery
        var correlationId = Guid.NewGuid().ToString();
        var trackingNumber = "SHIP24X7-20240101043";
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        using var shipmentPublisher = new ShipmentEventPublisher(configuration);
        using var paymentPublisher = new PaymentEventPublisher(configuration);
        using var trackingPublisher = new TrackingEventPublisher(configuration);
        
        using var consumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.correlation.chain",
            "shipment.booked",
            "payment.captured",
            "shipment.delivered");

        // Act - Simulate event chain with same CorrelationId
        // Step 1: Shipment booked
        await shipmentPublisher.PublishAsync(new ShipmentBookedEventDto
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = trackingNumber,
            CustomerId = Guid.NewGuid(),
            TotalCost = 3000.00m,
            Currency = "INR",
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        });

        await Task.Delay(500);

        // Step 2: Payment captured
        await paymentPublisher.PublishAsync(new PaymentCapturedEventDto
        {
            PaymentOrderId = Guid.NewGuid(),
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = trackingNumber,
            RazorpayPaymentId = "pay_chain_test",
            Amount = 3000.00m,
            Currency = "INR",
            CapturedAt = DateTime.UtcNow,
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        });

        await Task.Delay(500);

        // Step 3: Shipment delivered
        await trackingPublisher.PublishAsync(new ShipmentDeliveredEventDto
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = trackingNumber,
            DeliveryDate = DateTime.UtcNow,
            ReceivedBy = "Chain Test User",
            Latitude = 28.6139m,
            Longitude = 77.2090m,
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        });

        await Task.Delay(1000);

        // Assert - All events in the chain should have the same CorrelationId
        consumer.ReceivedEvents.Should().HaveCount(3);
        consumer.ReceivedEvents.All(e => e.CorrelationId == correlationId)
            .Should().BeTrue("All events in the chain should share the same CorrelationId for tracing");
        
        // Verify event order
        var events = consumer.ReceivedEvents.OrderBy(e => e.ReceivedAt).ToList();
        events[0].EventType.Should().Be("ShipmentBookedEventDto");
        events[1].EventType.Should().Be("PaymentCapturedEventDto");
        events[2].EventType.Should().Be("ShipmentDeliveredEventDto");
    }
}

// Event DTOs for testing
public class ShipmentBookedEventDto : IDomainEvent
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

public class PaymentCapturedEventDto : IDomainEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public Guid PaymentOrderId { get; set; }
    public Guid ShipmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public DateTime CapturedAt { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}

public class ShipmentDeliveredEventDto : IDomainEvent
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
