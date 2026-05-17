using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shared.Domain;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ship24X7.Shipment.Infrastructure.Messaging;

/// <summary>
/// ShipmentEventPublisher implementation. Provides functionality for the application.
/// </summary>
public class ShipmentEventPublisher : IShipmentEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName = "ship24x7.events";

    // Enums must be serialized as strings so consumers can deserialize them by name.
    // The ASP.NET JsonStringEnumConverter is only wired into the HTTP pipeline, so we
    // need our own options here for RabbitMQ message serialization.
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ShipmentEventPublisher(IConfiguration configuration)
    {
        var rabbitMqConnectionString = configuration["RabbitMQ:ConnectionString"] 
            ?? throw new InvalidOperationException("RabbitMQ connection string not configured");

        var factory = new ConnectionFactory
        {
            Uri = new Uri(rabbitMqConnectionString)
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
        var routingKey = GetRoutingKey(eventTypeName);
        
        // Diagnostic logging for debugging
        Console.WriteLine($"[ShipmentEventPublisher] Publishing event: {eventTypeName} -> routing key: {routingKey}");
        
        var message = JsonSerializer.Serialize(domainEvent, _jsonOptions);
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

    private static string GetRoutingKey(string eventName)
    {
        return eventName switch
        {
            // Test DTOs with "EventDto" suffix
            "ShipmentBookedEventDto" => "shipment.booked",
            "ShipmentCancelledEventDto" => "shipment.cancelled",
            "PickupScheduledEventDto" => "shipment.pickup.scheduled",
            "PickupCompletedEventDto" => "shipment.pickup.completed",
            "ShipmentStatusChangedEventDto" => "shipment.status.changed",
            
            // Event classes with "Event" suffix (domain events)
            "ShipmentBookedEvent" => "shipment.booked",
            "ShipmentCancelledEvent" => "shipment.cancelled",
            "PickupScheduledEvent" => "shipment.pickup.scheduled",
            "PickupCompletedEvent" => "shipment.pickup.completed",
            "ShipmentStatusChangedEvent" => "shipment.status.changed",
            
            // Backward compatibility: Event classes without "Event" suffix
            "ShipmentBooked" => "shipment.booked",
            "ShipmentCancelled" => "shipment.cancelled",
            "PickupScheduled" => "shipment.pickup.scheduled",
            "PickupCompleted" => "shipment.pickup.completed",
            "ShipmentStatusChanged" => "shipment.status.changed",
            "ShipmentOutForDelivery" => "shipment.outfordelivery",
            
            _ => $"shipment.{eventName.ToLowerInvariant()}"
        };
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}
