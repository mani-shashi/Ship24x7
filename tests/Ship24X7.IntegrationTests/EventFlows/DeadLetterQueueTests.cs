using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Ship24X7.IntegrationTests.Infrastructure;
using Ship24X7.Shipment.Infrastructure.Messaging;
using Ship24X7.Shared.Domain;
using Xunit;
using ShipmentBookedEvent = Ship24X7.IntegrationTests.EventFlows.ShipmentBookedEvent;

namespace Ship24X7.IntegrationTests.EventFlows;

/// <summary>
/// Integration tests for Dead Letter Queue (DLQ) routing
/// Tests: Failed event processing after 3 retries should route to DLQ
/// </summary>
public class DeadLetterQueueTests : IClassFixture<RabbitMqTestFixture>
{
    private readonly RabbitMqTestFixture _fixture;

    public DeadLetterQueueTests(RabbitMqTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Failed_Event_Should_Be_Routed_To_DLQ_After_Max_Retries()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var dlqName = "test.dlq";
        var queueName = "test.retry.queue";
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        // Declare DLQ
        _fixture.Channel!.QueueDeclare(
            queue: dlqName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Declare main queue with DLQ configuration
        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "" },
            { "x-dead-letter-routing-key", dlqName }
        };
        
        _fixture.Channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args);

        _fixture.Channel.QueueBind(queueName, "ship24x7.events", "shipment.booked");

        using var publisher = new ShipmentEventPublisher(configuration);

        var shipmentBookedEvent = new ShipmentBookedEvent
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20240101030",
            CustomerId = Guid.NewGuid(),
            TotalCost = 1500.00m,
            Currency = "INR",
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        };

        // Set up a failing consumer
        var retryCount = 0;
        var consumer = new EventingBasicConsumer(_fixture.Channel);
        consumer.Received += (model, ea) =>
        {
            retryCount++;
            
            if (retryCount <= 3)
            {
                // Simulate failure by rejecting the message
                _fixture.Channel.BasicNack(ea.DeliveryTag, false, retryCount < 3);
            }
        };

        _fixture.Channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

        // Act
        await publisher.PublishAsync(shipmentBookedEvent);

        // Wait for retries to complete
        await Task.Delay(5000);

        // Assert
        var dlqResult = _fixture.Channel.BasicGet(dlqName, false);
        dlqResult.Should().NotBeNull("Message should be in DLQ after max retries");
        
        if (dlqResult != null)
        {
            _fixture.Channel.BasicAck(dlqResult.DeliveryTag, false);
        }
    }

    [Fact]
    public async Task Event_Should_Be_Retried_With_Exponential_Backoff()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var queueName = "test.retry.backoff.queue";
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        _fixture.Channel!.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        _fixture.Channel.QueueBind(queueName, "ship24x7.events", "shipment.booked");

        using var publisher = new ShipmentEventPublisher(configuration);

        var shipmentBookedEvent = new ShipmentBookedEvent
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20240101031",
            CustomerId = Guid.NewGuid(),
            TotalCost = 2000.00m,
            Currency = "INR",
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        };

        var retryTimestamps = new List<DateTime>();
        var retryCount = 0;
        
        var consumer = new EventingBasicConsumer(_fixture.Channel);
        consumer.Received += async (model, ea) =>
        {
            retryTimestamps.Add(DateTime.UtcNow);
            var currentRetry = retryCount;
            retryCount++;
            
            if (currentRetry < 2)
            {
                // Simulate failure with exponential backoff delay
                _fixture.Channel.BasicAck(ea.DeliveryTag, false);
                
                // Apply exponential backoff before requeuing
                var delaySeconds = Math.Pow(2, currentRetry);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                
                // Republish with retry count
                var properties = _fixture.Channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.Headers = new Dictionary<string, object>
                {
                    { "RetryCount", currentRetry + 1 }
                };
                
                _fixture.Channel.BasicPublish(
                    exchange: "ship24x7.events",
                    routingKey: "shipment.booked",
                    basicProperties: properties,
                    body: ea.Body);
            }
            else
            {
                // Success on third retry
                _fixture.Channel.BasicAck(ea.DeliveryTag, false);
            }
        };

        _fixture.Channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

        // Act
        await publisher.PublishAsync(shipmentBookedEvent);

        // Wait for retries
        await Task.Delay(8000);

        // Assert
        retryTimestamps.Should().HaveCountGreaterThanOrEqualTo(2);
        
        // Verify exponential backoff (approximate)
        if (retryTimestamps.Count >= 2)
        {
            var firstDelay = (retryTimestamps[1] - retryTimestamps[0]).TotalSeconds;
            firstDelay.Should().BeGreaterThan(0.5, "First retry should have some delay");
        }
    }

    [Fact]
    public async Task DLQ_Message_Should_Preserve_Original_CorrelationId()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var dlqName = "test.dlq.correlation";
        var queueName = "test.retry.correlation.queue";
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        // Declare DLQ
        _fixture.Channel!.QueueDeclare(
            queue: dlqName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Declare main queue with DLQ
        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "" },
            { "x-dead-letter-routing-key", dlqName }
        };
        
        _fixture.Channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args);

        _fixture.Channel.QueueBind(queueName, "ship24x7.events", "shipment.booked");

        using var publisher = new ShipmentEventPublisher(configuration);

        var shipmentBookedEvent = new ShipmentBookedEvent
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20240101032",
            CustomerId = Guid.NewGuid(),
            TotalCost = 1800.00m,
            Currency = "INR",
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        };

        // Set up failing consumer
        var consumer = new EventingBasicConsumer(_fixture.Channel);
        consumer.Received += (model, ea) =>
        {
            // Always reject to force DLQ routing
            _fixture.Channel.BasicNack(ea.DeliveryTag, false, false);
        };

        _fixture.Channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

        // Act
        await publisher.PublishAsync(shipmentBookedEvent);

        // Wait for DLQ routing
        await Task.Delay(2000);

        // Assert
        var dlqResult = _fixture.Channel.BasicGet(dlqName, false);
        dlqResult.Should().NotBeNull();
        
        if (dlqResult != null)
        {
            // RabbitMQ stores string headers as byte arrays, need to convert
            var correlationIdBytes = dlqResult.BasicProperties.Headers?["CorrelationId"] as byte[];
            var dlqCorrelationId = correlationIdBytes != null ? Encoding.UTF8.GetString(correlationIdBytes) : "";
            dlqCorrelationId.Should().Be(correlationId, "DLQ message should preserve original CorrelationId");
            
            _fixture.Channel.BasicAck(dlqResult.DeliveryTag, false);
        }
    }

    [Fact]
    public async Task Successful_Processing_After_Retry_Should_Not_Route_To_DLQ()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString();
        var dlqName = "test.dlq.success";
        var queueName = "test.retry.success.queue";
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "RabbitMQ:ConnectionString", _fixture.ConnectionString }
            })
            .Build();

        // Declare DLQ
        _fixture.Channel!.QueueDeclare(
            queue: dlqName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Declare main queue with DLQ
        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", "" },
            { "x-dead-letter-routing-key", dlqName }
        };
        
        _fixture.Channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args);

        _fixture.Channel.QueueBind(queueName, "ship24x7.events", "shipment.booked");

        using var publisher = new ShipmentEventPublisher(configuration);

        var shipmentBookedEvent = new ShipmentBookedEvent
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20240101033",
            CustomerId = Guid.NewGuid(),
            TotalCost = 2200.00m,
            Currency = "INR",
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
            CorrelationId = correlationId,
            OccurredAt = DateTime.UtcNow
        };

        var attemptCount = 0;
        var consumer = new EventingBasicConsumer(_fixture.Channel);
        consumer.Received += (model, ea) =>
        {
            attemptCount++;
            
            if (attemptCount == 1)
            {
                // Fail first attempt
                _fixture.Channel.BasicNack(ea.DeliveryTag, false, true);
            }
            else
            {
                // Succeed on retry
                _fixture.Channel.BasicAck(ea.DeliveryTag, false);
            }
        };

        _fixture.Channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

        // Act
        await publisher.PublishAsync(shipmentBookedEvent);

        // Wait for processing
        await Task.Delay(3000);

        // Assert
        var dlqResult = _fixture.Channel.BasicGet(dlqName, false);
        dlqResult.Should().BeNull("Message should not be in DLQ after successful retry");
    }
}
