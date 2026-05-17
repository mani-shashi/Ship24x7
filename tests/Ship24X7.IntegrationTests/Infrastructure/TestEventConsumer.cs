using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ship24X7.IntegrationTests.Infrastructure;

/// <summary>
/// Test consumer for capturing events published to RabbitMQ
/// </summary>
public class TestEventConsumer : IDisposable
{
    private readonly IModel _channel;
    private readonly string _queueName;
    private readonly ConcurrentBag<ReceivedEvent> _receivedEvents = new();
    private readonly EventingBasicConsumer _consumer;

    public TestEventConsumer(IModel channel, string queueName, params string[] routingKeys)
    {
        _channel = channel;
        _queueName = queueName;

        // Declare queue
        _channel.QueueDeclare(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Bind to routing keys
        foreach (var routingKey in routingKeys)
        {
            _channel.QueueBind(_queueName, "ship24x7.events", routingKey);
        }

        // Set up consumer
        _consumer = new EventingBasicConsumer(_channel);
        _consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            // Extract headers - RabbitMQ stores string headers as byte arrays
            var eventType = GetHeaderValue(ea.BasicProperties.Headers, "EventType");
            var correlationId = GetHeaderValue(ea.BasicProperties.Headers, "CorrelationId");

            Console.WriteLine($"[TestEventConsumer] Received event: Type={eventType}, CorrelationId={correlationId}, RoutingKey={ea.RoutingKey}");

            _receivedEvents.Add(new ReceivedEvent
            {
                RoutingKey = ea.RoutingKey,
                EventType = eventType,
                CorrelationId = correlationId,
                Message = message,
                ReceivedAt = DateTime.UtcNow
            });

            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: _consumer);
        
        // Give the consumer a moment to fully register
        Thread.Sleep(100);
    }

    public IReadOnlyList<ReceivedEvent> ReceivedEvents => _receivedEvents.ToList();

    public async Task<ReceivedEvent?> WaitForEventAsync(string eventType, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow.Add(timeout);
        var startTime = DateTime.UtcNow;
        var lastLogTime = DateTime.MinValue;
        
        while (DateTime.UtcNow < deadline)
        {
            var evt = _receivedEvents.FirstOrDefault(e => e.EventType == eventType);
            if (evt != null)
            {
                var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
                Console.WriteLine($"[TestEventConsumer] Found event '{eventType}' after {elapsed:F0}ms");
                return evt;
            }
            
            // Log every second for debugging
            if ((DateTime.UtcNow - lastLogTime).TotalSeconds >= 1)
            {
                var receivedTypes = string.Join(", ", _receivedEvents.Select(e => e.EventType));
                Console.WriteLine($"[TestEventConsumer] Still waiting for '{eventType}'... (Received {_receivedEvents.Count} events: {receivedTypes})");
                lastLogTime = DateTime.UtcNow;
            }
            
            await Task.Delay(100);
        }
        
        var allReceivedTypes = string.Join(", ", _receivedEvents.Select(e => e.EventType));
        Console.WriteLine($"[TestEventConsumer] TIMEOUT waiting for '{eventType}'. Total received: {_receivedEvents.Count} events: [{allReceivedTypes}]");
        return null;
    }

    public void Clear()
    {
        _receivedEvents.Clear();
    }

    public void Dispose()
    {
        _channel.QueueDelete(_queueName);
    }

    private static string GetHeaderValue(IDictionary<string, object>? headers, string key)
    {
        if (headers == null || !headers.TryGetValue(key, out var value))
            return string.Empty;

        // RabbitMQ stores string headers as byte arrays
        if (value is byte[] bytes)
            return Encoding.UTF8.GetString(bytes);

        return value?.ToString() ?? string.Empty;
    }
}

public class ReceivedEvent
{
    public string RoutingKey { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
}
