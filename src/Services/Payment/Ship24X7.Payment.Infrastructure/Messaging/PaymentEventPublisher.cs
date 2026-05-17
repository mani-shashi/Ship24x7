using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Ship24X7.Payment.Application.Interfaces;
using Ship24X7.Shared.Domain;
using System.Text;
using System.Text.Json;

namespace Ship24X7.Payment.Infrastructure.Messaging;

/// <summary>
/// PaymentEventPublisher implementation. Provides functionality for the application.
/// </summary>
public class PaymentEventPublisher : IPaymentEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName = "ship24x7.events";

    public PaymentEventPublisher(IConfiguration configuration)
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
        Console.WriteLine($"[PaymentEventPublisher] Publishing event: {eventTypeName} -> routing key: {routingKey}");
        
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

    private static string GetRoutingKey(string eventName)
    {
        return eventName switch
        {
            // Test DTOs with "EventDto" suffix
            "PaymentCapturedEventDto" => "payment.captured",
            "PaymentFailedEventDto" => "payment.failed",
            "RefundProcessedEventDto" => "refund.processed",
            
            // Event classes with "Event" suffix (domain events)
            "PaymentCapturedEvent" => "payment.captured",
            "PaymentFailedEvent" => "payment.failed",
            "RefundProcessedEvent" => "refund.processed",
            
            // Backward compatibility: Event classes without "Event" suffix
            "PaymentCaptured" => "payment.captured",
            "PaymentFailed" => "payment.failed",
            "RefundProcessed" => "refund.processed",
            
            _ => $"payment.{eventName.ToLowerInvariant()}"
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
