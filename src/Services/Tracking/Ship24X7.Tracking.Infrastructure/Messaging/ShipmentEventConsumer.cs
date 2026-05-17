using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Ship24X7.Tracking.Application.Interfaces;
using Ship24X7.Tracking.Domain.Entities;
using Ship24X7.Tracking.Domain.Enums;

namespace Ship24X7.Tracking.Infrastructure.Messaging;

/// <summary>
/// Background service that listens for shipment lifecycle events from RabbitMQ and
/// automatically creates the first tracking event so the tracking page is never empty.
///
/// Subscribed routing keys:
///   payment.captured  → status Paid,   "Shipment booked and payment confirmed"
///   shipment.booked   → status Booked, "Shipment booked and awaiting payment"
///
/// Both events carry ShipmentId + TrackingNumber which is all we need to seed the
/// TrackingEvent table. Subsequent events (PickedUp, InTransit, etc.) are written
/// by hub operators via POST /api/v1/tracking/events.
/// </summary>
public class ShipmentEventConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ShipmentEventConsumer> _logger;
    private readonly string _rabbitMqConnectionString;

    private readonly string _exchangeName = "ship24x7.events";
    private readonly string _queueName = "tracking.shipment.events";

    private IConnection? _connection;
    private IModel? _channel;

    public ShipmentEventConsumer(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<ShipmentEventConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _rabbitMqConnectionString = configuration.GetConnectionString("RabbitMQ")
            ?? "amqp://guest:guest@localhost:5672";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[ShipmentEventConsumer] Service is starting...");
        
        try 
        {
            await StartConsuming(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "[ShipmentEventConsumer] Fatal error during startup");
        }
    }

    private async Task StartConsuming(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[ShipmentEventConsumer] Connecting to RabbitMQ at {Uri}...", _rabbitMqConnectionString);
        
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
        _channel.QueueBind(_queueName, _exchangeName, "shipment.booked");
        _channel.QueueBind(_queueName, _exchangeName, "shipment.status.changed");

        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) => await HandleMessageAsync(ea);

        _channel.BasicConsume(queue: _queueName, autoAck: false, consumer: consumer);

        _logger.LogInformation("[ShipmentEventConsumer] Successfully connected and listening on queue '{Queue}'", _queueName);

        // Keep the task alive until cancellation requested
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs ea)
    {
        try
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var correlationId = ea.BasicProperties.Headers?["CorrelationId"]?.ToString() ?? "";

            _logger.LogInformation(
                "[ShipmentEventConsumer] Received '{RoutingKey}' CorrelationId={CorrelationId}",
                ea.RoutingKey, correlationId);

            switch (ea.RoutingKey)
            {
                case "payment.captured":
                    await HandlePaymentCapturedAsync(message, correlationId);
                    break;
                case "shipment.booked":
                    await HandleShipmentBookedAsync(message, correlationId);
                    break;
                case "shipment.status.changed":
                    await HandleShipmentStatusChangedAsync(message, correlationId);
                    break;
                default:
                    _logger.LogWarning("[ShipmentEventConsumer] Unknown routing key: {Key}", ea.RoutingKey);
                    break;
            }

            _channel!.BasicAck(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ShipmentEventConsumer] Error processing message");
            _channel!.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }

    /// <summary>
    /// payment.captured → first tracking event: Paid.
    /// This is the primary path — most shipments go Draft → payment → Paid.
    /// Only creates the event if one doesn't already exist for this shipment,
    /// so re-delivered messages are idempotent.
    /// </summary>
    private async Task HandlePaymentCapturedAsync(string message, string correlationId)
    {
        var evt = JsonSerializer.Deserialize<PaymentCapturedMessage>(message);
        if (evt == null) return;

        await CreateTrackingEventIfNotExists(
            shipmentId: evt.ShipmentId,
            trackingNumber: evt.TrackingNumber,
            status: ShipmentStatus.Paid,
            description: "Shipment booked and payment confirmed. Awaiting pickup scheduling.",
            location: "Origin",
            correlationId: correlationId);
    }

    /// <summary>
    /// shipment.booked → first tracking event: Booked.
    /// Covers the case where ConfirmShipment is called (payment done externally / COD).
    /// Skipped if a Paid event already exists for the same shipment.
    /// </summary>
    private async Task HandleShipmentBookedAsync(string message, string correlationId)
    {
        var evt = JsonSerializer.Deserialize<ShipmentBookedMessage>(message);
        if (evt == null) return;

        await CreateTrackingEventIfNotExists(
            shipmentId: evt.ShipmentId,
            trackingNumber: evt.TrackingNumber,
            status: ShipmentStatus.Booked,
            description: "Shipment booked successfully. Awaiting payment confirmation.",
            location: "Origin",
            correlationId: correlationId);
    }

    /// <summary>
    /// shipment.status.changed → records every operational status transition
    /// (PickedUp, InTransit, OutForDelivery, Delivered, Delayed, Failed, etc.)
    /// so the tracking timeline stays in sync automatically without manual API calls.
    /// </summary>
    private async Task HandleShipmentStatusChangedAsync(string message, string correlationId)
    {
        var evt = JsonSerializer.Deserialize<ShipmentStatusChangedMessage>(message);
        if (evt == null) return;

        // Map string status to enum — the Shipment service serialises enums as strings.
        // When the parse fails, store the raw string in RawStatus and leave Status null
        // so the event is never silently dropped.
        var parsedStatus = StatusMapper.Parse(evt.NewStatus);
        if (parsedStatus is null)
        {
            _logger.LogWarning(
                "[ShipmentEventConsumer] Unknown status '{Status}' in ShipmentStatusChanged for {ShipmentId} — storing as RawStatus",
                evt.NewStatus, evt.ShipmentId);

            await CreateTrackingEventWithRawStatusIfNotExists(
                shipmentId:     evt.ShipmentId,
                trackingNumber: evt.TrackingNumber,
                rawStatus:      evt.NewStatus,
                description:    evt.Reason,
                location:       evt.Location,
                correlationId:  correlationId);
            return;
        }

        var status = parsedStatus.Value;
        var description = status switch
        {
            ShipmentStatus.Paid            => "Payment confirmed. Shipment is ready for pickup.",
            ShipmentStatus.PickedUp        => "Package picked up from sender.",
            ShipmentStatus.InTransit       => $"Package in transit. {evt.Reason}".TrimEnd('.', ' ') + ".",
            ShipmentStatus.OutForDelivery  => "Package is out for delivery.",
            ShipmentStatus.Delivered       => "Package delivered successfully.",
            ShipmentStatus.Delayed         => $"Shipment delayed. {evt.Reason}".TrimEnd('.', ' ') + ".",
            ShipmentStatus.Failed          => $"Delivery failed. {evt.Reason}".TrimEnd('.', ' ') + ".",
            ShipmentStatus.Returned        => "Package returned to sender.",
            ShipmentStatus.Cancelled       => "Shipment cancelled.",
            _                              => evt.Reason
        };

        await CreateTrackingEventIfNotExists(
            shipmentId:     evt.ShipmentId,
            trackingNumber: evt.TrackingNumber,
            status:         status,
            description:    description,
            location:       evt.Location,
            correlationId:  correlationId);
    }

    private async Task CreateTrackingEventIfNotExists(
        Guid shipmentId,
        string trackingNumber,
        ShipmentStatus status,
        string description,
        string location,
        string correlationId)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ITrackingEventRepository>();

        // Idempotency check — don't create duplicate events
        var existing = await repo.GetByShipmentIdAsync(shipmentId);
        if (existing.Any(e => e.Status == status))
        {
            _logger.LogInformation(
                "[ShipmentEventConsumer] Tracking event {Status} already exists for Shipment {ShipmentId}, skipping",
                status, shipmentId);
            return;
        }

        var trackingEvent = new TrackingEvent
        {
            Id = Guid.NewGuid(),
            ShipmentId = shipmentId,
            TrackingNumber = trackingNumber,
            Status = status,
            Description = description,
            Location = location,
            EventTimestamp = DateTime.UtcNow,
            IsException = false,
            RecordedBy = "system",
            CorrelationId = correlationId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.Empty
        };

        await repo.AddAsync(trackingEvent);

        _logger.LogInformation(
            "[ShipmentEventConsumer] Created tracking event {Status} for Shipment {ShipmentId} / {TrackingNumber}",
            status, shipmentId, trackingNumber);
    }

    /// <summary>
    /// Creates a tracking event with a raw (unrecognised) status string when the enum parse fails.
    /// Status is left null; RawStatus stores the original string for diagnostics.
    /// </summary>
    private async Task CreateTrackingEventWithRawStatusIfNotExists(
        Guid shipmentId,
        string trackingNumber,
        string rawStatus,
        string description,
        string location,
        string correlationId)
    {
        using var scope = _serviceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ITrackingEventRepository>();

        // Idempotency check — don't create duplicate raw-status events
        var existing = await repo.GetByShipmentIdAsync(shipmentId);
        if (existing.Any(e => e.RawStatus == rawStatus))
        {
            _logger.LogInformation(
                "[ShipmentEventConsumer] Tracking event with RawStatus '{RawStatus}' already exists for Shipment {ShipmentId}, skipping",
                rawStatus, shipmentId);
            return;
        }

        var trackingEvent = new TrackingEvent
        {
            Id = Guid.NewGuid(),
            ShipmentId = shipmentId,
            TrackingNumber = trackingNumber,
            Status = null,
            RawStatus = rawStatus,
            Description = description,
            Location = location,
            EventTimestamp = DateTime.UtcNow,
            IsException = false,
            RecordedBy = "system",
            CorrelationId = correlationId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.Empty
        };

        await repo.AddAsync(trackingEvent);

        _logger.LogInformation(
            "[ShipmentEventConsumer] Created tracking event with RawStatus '{RawStatus}' for Shipment {ShipmentId} / {TrackingNumber}",
            rawStatus, shipmentId, trackingNumber);
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

// ─── Inbound event DTOs ───────────────────────────────────────────────────────
// These mirror what the Payment and Shipment services publish to RabbitMQ.

internal sealed class PaymentCapturedMessage
{
    public Guid ShipmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

internal sealed class ShipmentBookedMessage
{
    public Guid ShipmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
}

internal sealed class ShipmentStatusChangedMessage
{
    public Guid ShipmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    /// <summary>Canonical contract alias for <see cref="OldStatus"/>. Populated by the Shipment service v1.0+ contract.</summary>
    public string PreviousStatus { get; set; } = string.Empty;
    /// <summary>Event schema version (e.g. "1.0"). Used to detect breaking contract changes.</summary>
    public string SchemaVersion { get; set; } = "1.0";
    public string Reason { get; set; } = string.Empty;
    /// <summary>Hub name or city populated by hub-triggered transitions.</summary>
    public string Location { get; set; } = string.Empty;
}
