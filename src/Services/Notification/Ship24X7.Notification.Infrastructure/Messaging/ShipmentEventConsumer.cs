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
/// ShipmentEventConsumer implementation. Provides functionality for the application.
/// </summary>
public class ShipmentEventConsumer : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ShipmentEventConsumer> _logger;
    private readonly string _exchangeName = "ship24x7.events";
    private readonly string _queueName = "notification.shipment.events";

    public ShipmentEventConsumer(string rabbitMqConnectionString, IServiceProvider serviceProvider, ILogger<ShipmentEventConsumer> logger)
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
        _channel.QueueBind(_queueName, _exchangeName, "shipment.booked");
        _channel.QueueBind(_queueName, _exchangeName, "shipment.outfordelivery");
        _channel.QueueBind(_queueName, _exchangeName, "shipment.delivered");
        _channel.QueueBind(_queueName, _exchangeName, "shipment.delayed");

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
            switch (ea.RoutingKey)
            {
                case "shipment.booked":
                    await HandleShipmentBookedAsync(message, mediator, correlationId);
                    break;
                case "shipment.outfordelivery":
                    await HandleShipmentOutForDeliveryAsync(message, mediator, correlationId);
                    break;
                case "shipment.delivered":
                    await HandleShipmentDeliveredAsync(message, mediator, correlationId);
                    break;
                case "shipment.delayed":
                    await HandleShipmentDelayedAsync(message, mediator, correlationId);
                    break;
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

    private async Task HandleShipmentBookedAsync(string message, IMediator mediator, string correlationId)
    {
        var shipmentBooked = JsonSerializer.Deserialize<ShipmentBookedEvent>(message);
        if (shipmentBooked == null) return;

        _logger.LogInformation("Processing ShipmentBooked event for {TrackingNumber} with CorrelationId {CorrelationId}", 
            shipmentBooked.TrackingNumber, correlationId);

        // TODO: Get user email and phone from user service
        var command = new SendNotificationCommand
        {
            UserId = shipmentBooked.CustomerId,
            RecipientEmail = "customer@example.com", // TODO: Get from user service
            RecipientPhone = "",
            Channel = NotificationChannel.Email,
            TemplateId = Guid.Empty, // TODO: Get template by type
            PlaceholderData = new Dictionary<string, string>
            {
                { "TrackingNumber", shipmentBooked.TrackingNumber },
                { "TotalCost", shipmentBooked.TotalCost.ToString("C") },
                { "EstimatedDeliveryDate", shipmentBooked.EstimatedDeliveryDate.ToString("yyyy-MM-dd") }
            },
            EventType = "ShipmentBooked",
            EventData = message
        };

        await mediator.Send(command);
        
        _logger.LogInformation("Booking confirmation email sent for {TrackingNumber}", shipmentBooked.TrackingNumber);
    }

    private async Task HandleShipmentDeliveredAsync(string message, IMediator mediator, string correlationId)
    {
        var shipmentDelivered = JsonSerializer.Deserialize<ShipmentDeliveredEvent>(message);
        if (shipmentDelivered == null) return;

        _logger.LogInformation("Processing ShipmentDelivered event for {TrackingNumber} with CorrelationId {CorrelationId}", 
            shipmentDelivered.TrackingNumber, correlationId);

        // TODO: Get user email and phone from user service
        // TODO: Check user notification preferences for SMS
        var command = new SendNotificationCommand
        {
            UserId = Guid.Empty, // TODO: Get from shipment
            RecipientEmail = "customer@example.com", // TODO: Get from user service
            RecipientPhone = "", // TODO: Get from user service if SMS enabled
            Channel = NotificationChannel.Email, // TODO: Send both Email and SMS if enabled
            TemplateId = Guid.Empty, // TODO: Get template by type
            PlaceholderData = new Dictionary<string, string>
            {
                { "TrackingNumber", shipmentDelivered.TrackingNumber },
                { "DeliveryDate", shipmentDelivered.DeliveryDate.ToString("yyyy-MM-dd HH:mm") }
            },
            EventType = "ShipmentDelivered",
            EventData = message
        };

        await mediator.Send(command);
        
        _logger.LogInformation("Delivery confirmation email sent for {TrackingNumber}", shipmentDelivered.TrackingNumber);
    }

    private async Task HandleShipmentDelayedAsync(string message, IMediator mediator, string correlationId)
    {
        var shipmentDelayed = JsonSerializer.Deserialize<ShipmentDelayedEvent>(message);
        if (shipmentDelayed == null) return;

        _logger.LogInformation("Processing ShipmentDelayed event for {TrackingNumber} with CorrelationId {CorrelationId}", 
            shipmentDelayed.TrackingNumber, correlationId);

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
                { "TrackingNumber", shipmentDelayed.TrackingNumber },
                { "Reason", shipmentDelayed.Reason }
            },
            EventType = "ShipmentDelayed",
            EventData = message
        };

        await mediator.Send(command);
        
        _logger.LogInformation("Delay notification email sent for {TrackingNumber}", shipmentDelayed.TrackingNumber);
    }

    private async Task HandleShipmentOutForDeliveryAsync(string message, IMediator mediator, string correlationId)
    {
        var evt = JsonSerializer.Deserialize<ShipmentOutForDeliveryEvent>(message);
        if (evt == null) return;

        _logger.LogInformation(
            "Processing ShipmentOutForDelivery for {TrackingNumber} CorrelationId {CorrelationId}",
            evt.TrackingNumber, correlationId);

        // SMS — primary channel so the customer gets the OTP on their phone
        var smsCommand = new SendNotificationCommand
        {
            UserId = evt.CustomerId,
            RecipientEmail = "",
            RecipientPhone = "",   // TODO: resolve from user service
            Channel = NotificationChannel.SMS,
            TemplateId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
            PlaceholderData = new Dictionary<string, string>
            {
                { "TrackingNumber", evt.TrackingNumber },
                { "DeliveryOtp",    evt.DeliveryOtp },
                { "OtpExpiresAt",   evt.OtpExpiresAt.ToString("HH:mm") },
                { "DeliveryAgent",  evt.DeliveryAgentId }
            },
            EventType = "ShipmentOutForDelivery",
            EventData = message
        };
        await mediator.Send(smsCommand);

        // Email — also sent so the OTP appears in the in-app notification center
        var emailCommand = new SendNotificationCommand
        {
            UserId = evt.CustomerId,
            RecipientEmail = "",   // TODO: resolve from user service
            RecipientPhone = "",
            Channel = NotificationChannel.Email,
            TemplateId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
            PlaceholderData = new Dictionary<string, string>
            {
                { "TrackingNumber", evt.TrackingNumber },
                { "DeliveryOtp",    evt.DeliveryOtp },
                { "OtpExpiresAt",   evt.OtpExpiresAt.ToString("HH:mm UTC") },
                { "DeliveryAgent",  evt.DeliveryAgentId }
            },
            EventType = "ShipmentOutForDelivery",
            EventData = message
        };
        await mediator.Send(emailCommand);

        _logger.LogInformation(
            "Out-for-delivery OTP notifications sent for {TrackingNumber}", evt.TrackingNumber);
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}

// Event DTOs
/// <summary>
/// ShipmentBookedEvent implementation. Provides functionality for the application.
/// </summary>
public class ShipmentBookedEvent
{
    /// <summary>
    /// Gets or sets the shipmentId.
    /// </summary>
    public Guid ShipmentId { get; set; }
    /// <summary>
    /// Gets or sets the trackingNumber.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the customerId.
    /// </summary>
    public Guid CustomerId { get; set; }
    /// <summary>
    /// Gets or sets the totalcost.
    /// </summary>
    public decimal TotalCost { get; set; }
    /// <summary>
    /// Gets or sets the currency.
    /// </summary>
    public string Currency { get; set; } = "INR";
    /// <summary>
    /// Gets or sets the estimateddeliverydate.
    /// </summary>
    public DateTime EstimatedDeliveryDate { get; set; }
}

/// <summary>
/// ShipmentDeliveredEvent implementation. Provides functionality for the application.
/// </summary>
public class ShipmentDeliveredEvent
{
    /// <summary>
    /// Gets or sets the shipmentId.
    /// </summary>
    public Guid ShipmentId { get; set; }
    /// <summary>
    /// Gets or sets the trackingNumber.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the deliverydate.
    /// </summary>
    public DateTime DeliveryDate { get; set; }
}

/// <summary>
/// ShipmentDelayedEvent implementation. Provides functionality for the application.
/// </summary>
public class ShipmentDelayedEvent
{
    public Guid ShipmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class ShipmentOutForDeliveryEvent
{
    public Guid ShipmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string DeliveryAgentId { get; set; } = string.Empty;
    /// <summary>Raw 6-digit OTP — consumed once to send to the customer, never stored.</summary>
    public string DeliveryOtp { get; set; } = string.Empty;
    public DateTime OtpExpiresAt { get; set; }
}
