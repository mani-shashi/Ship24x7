using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Ship24X7.Shipment.Application.Commands;
using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Infrastructure.Messaging;

/// <summary>
/// Background service that listens for payment events (payment.captured, payment.failed)
/// from RabbitMQ and updates shipment status accordingly.
/// Registered as IHostedService so ASP.NET Core starts it automatically on app startup.
/// </summary>
public class PaymentEventConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentEventConsumer> _logger;
    private readonly string _rabbitMqConnectionString;

    private readonly string _exchangeName = "ship24x7.events";
    private readonly string _queueName = "shipment.payment.events";

    private IConnection? _connection;
    private IModel? _channel;

    public PaymentEventConsumer(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<PaymentEventConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _rabbitMqConnectionString = configuration.GetConnectionString("RabbitMQ")
            ?? "amqp://guest:guest@localhost:5672";
    }

    /// <summary>
    /// Called by the runtime when the host starts.
    /// Connects to RabbitMQ, declares the exchange/queue, and begins consuming.
    /// </summary>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Connect on a background thread so startup isn't blocked if RabbitMQ is slow
        Task.Run(() => StartConsuming(stoppingToken), stoppingToken);
        return Task.CompletedTask;
    }

    private void StartConsuming(CancellationToken stoppingToken)
    {
        try
        {
            var factory = new ConnectionFactory { Uri = new Uri(_rabbitMqConnectionString) };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(
                exchange: _exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            _channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueBind(_queueName, _exchangeName, "payment.captured");
            _channel.QueueBind(_queueName, _exchangeName, "payment.failed");

            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) => await HandleMessageAsync(ea);

            _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

            _logger.LogInformation("[PaymentEventConsumer] Listening on queue '{Queue}'", _queueName);

            // Keep the thread alive until the host shuts down
            stoppingToken.WaitHandle.WaitOne();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PaymentEventConsumer] Failed to connect to RabbitMQ");
        }
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs ea)
    {
        var retryCount = 0;
        if (ea.BasicProperties.Headers != null &&
            ea.BasicProperties.Headers.TryGetValue("RetryCount", out var rc))
        {
            retryCount = Convert.ToInt32(rc);
        }

        try
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var correlationId = ea.BasicProperties.Headers?["CorrelationId"]?.ToString() ?? "";

            _logger.LogInformation(
                "[PaymentEventConsumer] Received '{RoutingKey}' CorrelationId={CorrelationId}",
                ea.RoutingKey, correlationId);

            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            switch (ea.RoutingKey)
            {
                case "payment.captured":
                    await HandlePaymentCapturedAsync(message, mediator, correlationId);
                    break;
                case "payment.failed":
                    await HandlePaymentFailedAsync(message, mediator, correlationId);
                    break;
                default:
                    _logger.LogWarning("[PaymentEventConsumer] Unknown routing key: {Key}", ea.RoutingKey);
                    break;
            }

            _channel!.BasicAck(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PaymentEventConsumer] Error processing message (retry {Retry})", retryCount);

            if (retryCount < 3)
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));

                var props = _channel!.CreateBasicProperties();
                props.Headers = new Dictionary<string, object> { { "RetryCount", retryCount + 1 } };

                _channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: ea.RoutingKey,
                    basicProperties: props,
                    body: ea.Body);

                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            else
            {
                _logger.LogError("[PaymentEventConsumer] Moving message to DLQ after {Retries} retries", retryCount);
                _channel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        }
    }

    private async Task HandlePaymentCapturedAsync(string message, IMediator mediator, string correlationId)
    {
        var evt = JsonSerializer.Deserialize<PaymentCapturedEvent>(message);
        if (evt == null) return;

        _logger.LogInformation(
            "[PaymentEventConsumer] PaymentCaptured for Shipment {ShipmentId} CorrelationId={CorrelationId}",
            evt.ShipmentId, correlationId);

        await mediator.Send(new UpdateShipmentStatusCommand
        {
            ShipmentId = evt.ShipmentId,
            NewStatus = ShipmentStatus.Paid,
            Reason = $"Payment captured: {evt.RazorpayPaymentId}"
        });

        _logger.LogInformation("[PaymentEventConsumer] Shipment {ShipmentId} → Paid", evt.ShipmentId);
    }

    private async Task HandlePaymentFailedAsync(string message, IMediator mediator, string correlationId)
    {
        var evt = JsonSerializer.Deserialize<PaymentFailedEvent>(message);
        if (evt == null) return;

        _logger.LogInformation(
            "[PaymentEventConsumer] PaymentFailed for Shipment {ShipmentId} CorrelationId={CorrelationId}",
            evt.ShipmentId, correlationId);

        await mediator.Send(new UpdateShipmentStatusCommand
        {
            ShipmentId = evt.ShipmentId,
            NewStatus = ShipmentStatus.PaymentFailed,
            Reason = $"Payment failed: {evt.FailureReason}"
        });

        _logger.LogInformation("[PaymentEventConsumer] Shipment {ShipmentId} → PaymentFailed", evt.ShipmentId);
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        base.Dispose();
    }
}

// ─── Event DTOs ──────────────────────────────────────────────────────────────

public class PaymentCapturedEvent
{
    public Guid PaymentOrderId { get; set; }
    public Guid ShipmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public DateTime CapturedAt { get; set; }
}

public class PaymentFailedEvent
{
    public Guid PaymentOrderId { get; set; }
    public Guid ShipmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string FailureReason { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
}
