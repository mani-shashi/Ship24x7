using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Ship24X7.Notification.Application.Commands;
using Ship24X7.Notification.Domain.ValueObjects;
using MediatR;

namespace Ship24X7.Notification.Infrastructure.Messaging;

/// <summary>
/// PaymentEventConsumer implementation. Provides functionality for the application.
/// </summary>
public class PaymentEventConsumer : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentEventConsumer> _logger;
    private readonly string _exchangeName = "ship24x7.events";
    private readonly string _queueName = "notification.payment.events";

    public PaymentEventConsumer(string rabbitMqConnectionString, IServiceProvider serviceProvider, ILogger<PaymentEventConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

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

        // Declare queue
        _channel.QueueDeclare(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        // Bind queue to exchange with routing keys
        _channel.QueueBind(_queueName, _exchangeName, "payment.captured");

        // Set up consumer
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            await HandleMessageAsync(ea);
        };

        _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs ea)
    {
        var retryCount = 0;
        if (ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.ContainsKey("RetryCount"))
        {
            retryCount = Convert.ToInt32(ea.BasicProperties.Headers["RetryCount"]);
        }

        try
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var eventType = ea.BasicProperties.Headers?["EventType"]?.ToString() ?? "";
            var correlationId = ea.BasicProperties.Headers?["CorrelationId"]?.ToString() ?? "";

            _logger.LogInformation("Received event {EventType} with CorrelationId {CorrelationId}", eventType, correlationId);

            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // Process based on routing key
            if (ea.RoutingKey == "payment.captured")
            {
                await HandlePaymentCapturedAsync(message, mediator, correlationId);
            }

            // Acknowledge message
            _channel.BasicAck(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");

            // Retry logic with exponential backoff
            if (retryCount < 3)
            {
                // Requeue with delay
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));
                
                var properties = _channel.CreateBasicProperties();
                properties.Headers = new Dictionary<string, object>
                {
                    { "RetryCount", retryCount + 1 }
                };
                
                _channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: ea.RoutingKey,
                    basicProperties: properties,
                    body: ea.Body);
                
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            else
            {
                // Move to dead letter queue
                _logger.LogError("Message moved to DLQ after {RetryCount} retries", retryCount);
                _channel.BasicNack(ea.DeliveryTag, false, false);
            }
        }
    }

    private async Task HandlePaymentCapturedAsync(string message, IMediator mediator, string correlationId)
    {
        var paymentCaptured = JsonSerializer.Deserialize<PaymentCapturedEvent>(message);
        if (paymentCaptured == null) return;

        _logger.LogInformation("Processing PaymentCaptured event for {TrackingNumber} with CorrelationId {CorrelationId}", 
            paymentCaptured.TrackingNumber, correlationId);

        // TODO: Get user email from user service
        var command = new SendNotificationCommand
        {
            UserId = Guid.Empty, // TODO: Get from shipment
            RecipientEmail = "customer@example.com", // TODO: Get from user service
            RecipientPhone = "",
            Channel = NotificationChannel.Email,
            TemplateId = Guid.Empty, // TODO: Get template by type
            PlaceholderData = new Dictionary<string, string>
            {
                { "Amount", paymentCaptured.Amount.ToString("C") },
                { "PaymentId", paymentCaptured.RazorpayPaymentId },
                { "TrackingNumber", paymentCaptured.TrackingNumber }
            },
            EventType = "PaymentCaptured",
            EventData = message
        };

        await mediator.Send(command);
        
        _logger.LogInformation("Payment confirmation email sent for {TrackingNumber}", paymentCaptured.TrackingNumber);
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}

// Event DTO
/// <summary>
/// PaymentCapturedEvent implementation. Provides functionality for the application.
/// </summary>
public class PaymentCapturedEvent
{
    /// <summary>
    /// Gets or sets the Payment orderId.
    /// </summary>
    public Guid PaymentOrderId { get; set; }
    /// <summary>
    /// Gets or sets the shipmentId.
    /// </summary>
    public Guid ShipmentId { get; set; }
    /// <summary>
    /// Gets or sets the trackingNumber.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Razorpay paymentId.
    /// </summary>
    public string RazorpayPaymentId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the amount.
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// Gets or sets the currency.
    /// </summary>
    public string Currency { get; set; } = "INR";
    /// <summary>
    /// Gets or sets the capturedat.
    /// </summary>
    public DateTime CapturedAt { get; set; }
}
