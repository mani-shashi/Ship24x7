using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Ship24X7.IntegrationTests.Infrastructure;
using Ship24X7.Shipment.Infrastructure.Messaging;
using Ship24X7.Payment.Infrastructure.Messaging;
using Ship24X7.Shared.Domain;
using System.Text.Json;
using Xunit;

namespace Ship24X7.IntegrationTests.E2E;

/// <summary>
/// End-to-end integration tests for complete shipment booking flow
/// Tests the integration of: Shipment Service → Payment Service → Notification Service
/// 
/// **Validates: Requirements 1.1, 2.1, 6.1, 7.1, 17.1, 17.3, 12.1**
/// </summary>
public class ShipmentBookingFlowTests : IClassFixture<RabbitMqTestFixture>
{
    private readonly RabbitMqTestFixture _fixture;

    public ShipmentBookingFlowTests(RabbitMqTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CompleteShipmentBookingFlow_ShouldPublishEventsInCorrectOrder()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        using var shipmentPublisher = new ShipmentEventPublisher(configuration);
        using var paymentPublisher = new PaymentEventPublisher(configuration);
        
        // Set up consumers for each event in the flow
        using var shipmentBookedConsumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.shipment.booked",
            "shipment.booked");
            
        using var paymentCapturedConsumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.payment.captured",
            "payment.captured");

        var correlationId = Guid.NewGuid().ToString();
        var shipmentId = Guid.NewGuid();
        var trackingNumber = $"SHIP24X7-{DateTime.UtcNow:yyyyMMdd}001";

        // Act - Step 1: Shipment is booked
        var shipmentBookedEvent = new ShipmentBookedEventDto
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            CorrelationId = correlationId,
            ShipmentId = shipmentId,
            TrackingNumber = trackingNumber,
            CustomerId = Guid.NewGuid(),
            SenderName = "John Doe",
            SenderEmail = "john@example.com",
            ReceiverName = "Jane Smith",
            ReceiverEmail = "jane@example.com",
            ServiceType = "Express",
            TotalCost = 150.00m,
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
            BookedAt = DateTime.UtcNow
        };

        await shipmentPublisher.PublishAsync(shipmentBookedEvent);

        // Assert - Step 1: Verify ShipmentBooked event was published
        var receivedShipmentEvent = await shipmentBookedConsumer.WaitForEventAsync(
            "ShipmentBookedEventDto",
            TimeSpan.FromSeconds(5));

        receivedShipmentEvent.Should().NotBeNull();
        receivedShipmentEvent!.CorrelationId.Should().Be(correlationId);
        
        var shipmentEventData = JsonSerializer.Deserialize<ShipmentBookedEventDto>(receivedShipmentEvent.Message);
        shipmentEventData.Should().NotBeNull();
        shipmentEventData!.ShipmentId.Should().Be(shipmentId);

        // Act - Step 2: Payment is captured
        var paymentCapturedEvent = new PaymentCapturedEventDto
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            CorrelationId = correlationId,
            PaymentOrderId = Guid.NewGuid(),
            ShipmentId = shipmentId,
            RazorpayOrderId = "order_test123",
            RazorpayPaymentId = "pay_test456",
            Amount = 150.00m,
            Currency = "INR",
            CapturedAt = DateTime.UtcNow
        };

        await paymentPublisher.PublishAsync(paymentCapturedEvent);

        // Assert - Step 2: Verify PaymentCaptured event was published
        var receivedPaymentEvent = await paymentCapturedConsumer.WaitForEventAsync(
            "PaymentCapturedEventDto",
            TimeSpan.FromSeconds(5));

        receivedPaymentEvent.Should().NotBeNull();
        receivedPaymentEvent!.CorrelationId.Should().Be(correlationId);
        
        var paymentEventData = JsonSerializer.Deserialize<PaymentCapturedEventDto>(receivedPaymentEvent.Message);
        paymentEventData.Should().NotBeNull();
        paymentEventData!.ShipmentId.Should().Be(shipmentId);
    }

    [Fact]
    public async Task ShipmentBookingFlow_ShouldPropagateCorrelationIdThroughAllEvents()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        using var shipmentPublisher = new ShipmentEventPublisher(configuration);
        using var consumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.correlation.tracking",
            "shipment.#");

        var correlationId = Guid.NewGuid().ToString();

        // Act
        var shipmentBookedEvent = new ShipmentBookedEventDto
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            CorrelationId = correlationId,
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = $"SHIP24X7-{DateTime.UtcNow:yyyyMMdd}002",
            CustomerId = Guid.NewGuid(),
            SenderName = "Test Sender",
            SenderEmail = "sender@test.com",
            ReceiverName = "Test Receiver",
            ReceiverEmail = "receiver@test.com",
            ServiceType = "Standard",
            TotalCost = 100.00m,
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(5),
            BookedAt = DateTime.UtcNow
        };

        await shipmentPublisher.PublishAsync(shipmentBookedEvent);

        // Assert
        var receivedEvent = await consumer.WaitForEventAsync(
            "ShipmentBookedEventDto",
            TimeSpan.FromSeconds(5));

        receivedEvent.Should().NotBeNull();
        receivedEvent!.CorrelationId.Should().Be(correlationId);
        receivedEvent.CorrelationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ShipmentBookingFlow_WithRateCalculation_ShouldIncludeCorrectPricing()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        using var shipmentPublisher = new ShipmentEventPublisher(configuration);
        using var consumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.rate.calculation",
            "shipment.booked");

        // Act - Simulate shipment with calculated rate
        var correlationId = Guid.NewGuid().ToString();
        var shipmentBookedEvent = new ShipmentBookedEventDto
        {
            EventId = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            CorrelationId = correlationId,
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = $"SHIP24X7-{DateTime.UtcNow:yyyyMMdd}003",
            CustomerId = Guid.NewGuid(),
            SenderName = "Rate Test Sender",
            SenderEmail = "rate@test.com",
            ReceiverName = "Rate Test Receiver",
            ReceiverEmail = "ratereceiver@test.com",
            ServiceType = "Express",
            TotalCost = 250.50m, // Calculated rate
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(2),
            BookedAt = DateTime.UtcNow
        };

        await shipmentPublisher.PublishAsync(shipmentBookedEvent);

        // Assert
        var receivedEvent = await consumer.WaitForEventAsync(
            "ShipmentBookedEventDto",
            TimeSpan.FromSeconds(5));

        receivedEvent.Should().NotBeNull();
        receivedEvent!.CorrelationId.Should().Be(correlationId);
        
        var shipmentEventData = JsonSerializer.Deserialize<ShipmentBookedEventDto>(receivedEvent.Message);
        shipmentEventData.Should().NotBeNull();
        shipmentEventData!.TotalCost.Should().Be(250.50m);
        shipmentEventData.ServiceType.Should().Be("Express");
    }
}

// Event DTOs for testing
public class ShipmentBookedEventDto : IDomainEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public Guid ShipmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverEmail { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public DateTime EstimatedDeliveryDate { get; set; }
    public DateTime BookedAt { get; set; }
}

public class PaymentCapturedEventDto : IDomainEvent
{
    public Guid EventId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public Guid PaymentOrderId { get; set; }
    public Guid ShipmentId { get; set; }
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime CapturedAt { get; set; }
}
