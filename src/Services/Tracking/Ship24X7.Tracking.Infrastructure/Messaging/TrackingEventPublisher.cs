using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Ship24X7.Shared.Domain;

namespace Ship24X7.Tracking.Infrastructure.Messaging;

/// <summary>
/// TrackingEventPublisher implementation. Provides functionality for the application.
/// </summary>
public class TrackingEventPublisher : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName = "ship24x7.events";

    public TrackingEventPublisher(IConfiguration configuration)
    {
        var connectionString = configuration["RabbitMQ:ConnectionString"] ?? "amqp://guest:guest@localhost:5672";
        
        var factory = new ConnectionFactory
        {
            Uri = new Uri(connectionString)
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare exchange
        _channel.ExchangeDeclare(
            exchange: _exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);
    }

    public Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
    {
        var eventTypeName = typeof(TEvent).Name;
        var routingKey = GetRoutingKey<TEvent>();
        
        // Diagnostic logging for debugging
        Console.WriteLine($"[TrackingEventPublisher] Publishing event: {eventTypeName} -> routing key: {routingKey}");
        
        var message = JsonSerializer.Serialize(domainEvent);
        var body = Encoding.UTF8.GetBytes(message);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.Headers = new Dictionary<string, object>
        {
            { "EventType", eventTypeName },
            { "CorrelationId", domainEvent.CorrelationId }
        };

        _channel.BasicPublish(
            exchange: _exchangeName,
            routingKey: routingKey,
            basicProperties: properties,
            body: body);
            
        return Task.CompletedTask;
    }

    public void PublishEvent<TEvent>(TEvent domainEvent, string routingKey) where TEvent : IDomainEvent
    {
        var message = JsonSerializer.Serialize(domainEvent);
        var body = Encoding.UTF8.GetBytes(message);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.Headers = new Dictionary<string, object>
        {
            { "EventType", typeof(TEvent).Name },
            { "CorrelationId", domainEvent.CorrelationId }
        };

        _channel.BasicPublish(
            exchange: _exchangeName,
            routingKey: routingKey,
            basicProperties: properties,
            body: body);
    }

    private string GetRoutingKey<TEvent>() where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent).Name;
        return eventType switch
        {
            // Test DTOs with "EventDto" suffix
            "ShipmentDeliveredEventDto" => "shipment.delivered",
            "ShipmentDelayedEventDto" => "shipment.delayed",
            "TrackingEventRecordedDto" => "tracking.event.recorded",
            "DocumentUploadedDto" => "document.uploaded",
            
            // Event classes with "Event" suffix (domain events)
            "ShipmentDeliveredEvent" => "shipment.delivered",
            "ShipmentDelayedEvent" => "shipment.delayed",
            "TrackingEventRecorded" => "tracking.event.recorded",
            "DocumentUploaded" => "document.uploaded",
            
            _ => eventType.ToLowerInvariant()
        };
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}
