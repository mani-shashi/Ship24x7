// Feature: true-microservices-refactor, Property 2: ShipmentStatusChangedEvent contains all required contract fields

using System.Text.Json;
using System.Text.Json.Serialization;
using FsCheck;
using FsCheck.Xunit;
using Ship24X7.Shipment.Domain.Enums;
using Ship24X7.Shipment.Domain.Events;

namespace Ship24X7.Shipment.Tests.EventContract;

/// <summary>
/// Property-based tests verifying that ShipmentStatusChangedEvent contains all required contract fields.
/// </summary>
/// <remarks>
/// Validates: Requirements 1.2
/// </remarks>
public class ShipmentContractFieldsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Property 2: For any valid ShipmentStatusChanged instance, the serialized JSON must contain
    /// all seven required contract fields — each non-null and non-empty:
    /// shipmentId, trackingNumber, status, previousStatus, occurredAt, correlationId, schemaVersion.
    /// </summary>
    [Property(MaxTest = 200)]
    public Property AllRequiredContractFields_ArePresent_NonNullAndNonEmpty()
    {
        // Generator for non-empty strings (at least 1 character)
        var nonEmptyStringGen = Arb.Default.NonEmptyString().Generator
            .Select(s => s.Get);

        // Generator for ShipmentStatus enum values
        var statusGen = Gen.Elements(Enum.GetValues<ShipmentStatus>());

        // Combine generators to produce arbitrary ShipmentStatusChanged instances
        var gen =
            from shipmentId in Arb.Default.Guid().Generator
            from trackingNumber in nonEmptyStringGen
            from newStatus in statusGen
            from oldStatus in statusGen
            from correlationId in Arb.Default.Guid().Generator
            select new ShipmentStatusChanged
            {
                ShipmentId = shipmentId,
                TrackingNumber = trackingNumber,
                NewStatus = newStatus,
                OldStatus = oldStatus,
                CorrelationId = correlationId.ToString(),
                Reason = "test",
                Location = "Test Hub"
                // SchemaVersion defaults to "1.0"
                // OccurredAt is set automatically by BaseDomainEvent
            };

        var arb = Arb.From(gen);

        return Prop.ForAll(arb, evt =>
        {
            var json = JsonSerializer.Serialize(evt, JsonOptions);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Check each of the seven required contract fields

            // 1. shipmentId
            var hasShipmentId = root.TryGetProperty("shipmentId", out var shipmentIdEl)
                && shipmentIdEl.ValueKind != JsonValueKind.Null
                && !string.IsNullOrEmpty(shipmentIdEl.GetString());

            // 2. trackingNumber
            var hasTrackingNumber = root.TryGetProperty("trackingNumber", out var trackingNumberEl)
                && trackingNumberEl.ValueKind != JsonValueKind.Null
                && !string.IsNullOrEmpty(trackingNumberEl.GetString());

            // 3. status
            var hasStatus = root.TryGetProperty("status", out var statusEl)
                && statusEl.ValueKind != JsonValueKind.Null
                && !string.IsNullOrEmpty(statusEl.GetString());

            // 4. previousStatus
            var hasPreviousStatus = root.TryGetProperty("previousStatus", out var previousStatusEl)
                && previousStatusEl.ValueKind != JsonValueKind.Null
                && !string.IsNullOrEmpty(previousStatusEl.GetString());

            // 5. occurredAt
            var hasOccurredAt = root.TryGetProperty("occurredAt", out var occurredAtEl)
                && occurredAtEl.ValueKind != JsonValueKind.Null
                && !string.IsNullOrEmpty(occurredAtEl.GetString());

            // 6. correlationId
            var hasCorrelationId = root.TryGetProperty("correlationId", out var correlationIdEl)
                && correlationIdEl.ValueKind != JsonValueKind.Null
                && !string.IsNullOrEmpty(correlationIdEl.GetString());

            // 7. schemaVersion
            var hasSchemaVersion = root.TryGetProperty("schemaVersion", out var schemaVersionEl)
                && schemaVersionEl.ValueKind != JsonValueKind.Null
                && !string.IsNullOrEmpty(schemaVersionEl.GetString());

            return hasShipmentId.Label("shipmentId must be present, non-null, and non-empty")
                .And(hasTrackingNumber.Label("trackingNumber must be present, non-null, and non-empty"))
                .And(hasStatus.Label("status must be present, non-null, and non-empty"))
                .And(hasPreviousStatus.Label("previousStatus must be present, non-null, and non-empty"))
                .And(hasOccurredAt.Label("occurredAt must be present, non-null, and non-empty"))
                .And(hasCorrelationId.Label("correlationId must be present, non-null, and non-empty"))
                .And(hasSchemaVersion.Label("schemaVersion must be present, non-null, and non-empty"));
        });
    }

    /// <summary>
    /// Property 2 (cross-check): The schemaVersion field must always equal "1.0" (the default).
    /// </summary>
    [Property(MaxTest = 100)]
    public Property SchemaVersion_IsAlways_DefaultValue()
    {
        var statusGen = Gen.Elements(Enum.GetValues<ShipmentStatus>());

        var gen =
            from newStatus in statusGen
            from oldStatus in statusGen
            select new ShipmentStatusChanged
            {
                ShipmentId = Guid.NewGuid(),
                TrackingNumber = "SHP-TEST-CONTRACT",
                NewStatus = newStatus,
                OldStatus = oldStatus,
                CorrelationId = Guid.NewGuid().ToString()
            };

        var arb = Arb.From(gen);

        return Prop.ForAll(arb, evt =>
        {
            var json = JsonSerializer.Serialize(evt, JsonOptions);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var schemaVersion = root.GetProperty("schemaVersion").GetString();

            return (schemaVersion == "1.0")
                .Label($"schemaVersion was '{schemaVersion}' but expected '1.0'");
        });
    }
}
