using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Ship24X7.IntegrationTests.Infrastructure;
using Ship24X7.Payment.Infrastructure.Messaging;
using Ship24X7.Shared.Domain;
using Xunit;

namespace Ship24X7.IntegrationTests.EventFlows;

/// <summary>
/// Integration tests for PaymentCaptured event flow
/// Tests: Payment Service -> RabbitMQ -> Shipment Service + Notification Service
/// </summary>
public class PaymentCapturedEventFlowTests : IClassFixture<RabbitMqTestFixture>
{
    private readonly RabbitMqTestFixture _fixture;

    public PaymentCapturedEventFlowTests(RabbitMqTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task PaymentCaptured_Event_Should_Be_Published_To_RabbitMQ()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var shipmentId = Guid.NewGuid();
        var paymentOrderId = Guid.NewGuid();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        using var publisher = new PaymentEventPublisher(configuration);
        using var consumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.payment.captured",
            "payment.captured");

        var paymentCapturedEvent = new PaymentCapturedEvent
        {
            PaymentOrderId = paymentOrderId,
            ShipmentId = shipmentId,
            TrackingNumber = "SHIP24X7-20240101010",
            RazorpayPaymentId = "pay_test123",
            Amount = 2500.00m,
            Currency = "INR",
            CapturedAt = DateTime.UtcNow,
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        };

        // Act
        await publisher.PublishAsync(paymentCapturedEvent);

        // Assert
        var receivedEvent = await consumer.WaitForEventAsync("PaymentCapturedEvent", TimeSpan.FromSeconds(5));
        
        receivedEvent.Should().NotBeNull();
        receivedEvent!.RoutingKey.Should().Be("payment.captured");
        receivedEvent.EventType.Should().Be("PaymentCapturedEvent");
        receivedEvent.CorrelationId.Should().Be(correlationId);
        
        var deserializedEvent = JsonSerializer.Deserialize<PaymentCapturedEvent>(receivedEvent.Message);
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.ShipmentId.Should().Be(shipmentId);
        deserializedEvent.PaymentOrderId.Should().Be(paymentOrderId);
        deserializedEvent.Amount.Should().Be(2500.00m);
    }

    [Fact]
    public async Task PaymentCaptured_Event_Should_Be_Consumed_By_Multiple_Services()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        using var publisher = new PaymentEventPublisher(configuration);
        
        // Simulate two different service consumers
        using var shipmentConsumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.shipment.payment.captured",
            "payment.captured");
        
        using var notificationConsumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.notification.payment.captured",
            "payment.captured");

        var paymentCapturedEvent = new PaymentCapturedEvent
        {
            PaymentOrderId = Guid.NewGuid(),
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20240101011",
            RazorpayPaymentId = "pay_test456",
            Amount = 3000.00m,
            Currency = "INR",
            CapturedAt = DateTime.UtcNow,
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        };

        // Act
        await publisher.PublishAsync(paymentCapturedEvent);

        // Assert - Both consumers should receive the event
        var shipmentEvent = await shipmentConsumer.WaitForEventAsync("PaymentCapturedEvent", TimeSpan.FromSeconds(5));
        var notificationEvent = await notificationConsumer.WaitForEventAsync("PaymentCapturedEvent", TimeSpan.FromSeconds(5));
        
        shipmentEvent.Should().NotBeNull();
        notificationEvent.Should().NotBeNull();
        
        shipmentEvent!.CorrelationId.Should().Be(correlationId);
        notificationEvent!.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public async Task PaymentCaptured_Event_Should_Include_Razorpay_Payment_Details()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var razorpayPaymentId = "pay_test789";
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        using var publisher = new PaymentEventPublisher(configuration);
        using var consumer = new TestEventConsumer(
            _fixture.Channel!,
            "test.payment.captured.details",
            "payment.captured");

        var paymentCapturedEvent = new PaymentCapturedEvent
        {
            PaymentOrderId = Guid.NewGuid(),
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20240101012",
            RazorpayPaymentId = razorpayPaymentId,
            Amount = 1800.00m,
            Currency = "INR",
            CapturedAt = DateTime.UtcNow,
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        };

        // Act
        await publisher.PublishAsync(paymentCapturedEvent);

        // Assert
        var receivedEvent = await consumer.WaitForEventAsync("PaymentCapturedEvent", TimeSpan.FromSeconds(5));
        
        receivedEvent.Should().NotBeNull();
        
        var deserializedEvent = JsonSerializer.Deserialize<PaymentCapturedEvent>(receivedEvent!.Message);
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.RazorpayPaymentId.Should().Be(razorpayPaymentId);
        deserializedEvent.CapturedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }
}

// Event DTO for testing
public class PaymentCapturedEvent : IDomainEvent
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
