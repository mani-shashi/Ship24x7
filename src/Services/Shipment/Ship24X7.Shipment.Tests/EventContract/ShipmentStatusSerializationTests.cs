// Feature: true-microservices-refactor, Property 1: ShipmentStatusChangedEvent serializes status as a string

using System.Text.Json;
using System.Text.Json.Serialization;
using FsCheck;
using FsCheck.Xunit;
using Ship24X7.Shipment.Domain.Enums;
using Ship24X7.Shipment.Domain.Events;

namespace Ship24X7.Shipment.Tests.EventContract;

/// <summary>
/// Property-based tests for the ShipmentStatusChangedEvent JSON serialization contract.
/// </summary>
/// <remarks>
/// Validates: Requirements 1.1
/// </remarks>
public class ShipmentStatusSerializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Property 1: For any ShipmentStatus enum value, the serialized JSON must contain
    /// a "status" field whose ValueKind is String (not Number).
    /// </summary>
    [Property(MaxTest = 200)]
    public Property StatusField_SerializesAsString_ForAnyEnumValue()
    {
        // Generate arbitrary ShipmentStatus enum values from the defined members
        var gen = Gen.Elements(Enum.GetValues<ShipmentStatus>());
        var arb = Arb.From(gen);

        return Prop.ForAll(arb, status =>
        {
            var evt = new ShipmentStatusChanged
            {
                ShipmentId = Guid.NewGuid(),
                TrackingNumber = "SHP-TEST-0001",
                NewStatus = status,
                OldStatus = status,
                Reason = "test",
                Location = "Test Hub"
            };

            var json = JsonSerializer.Serialize(evt, JsonOptions);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Property 1: "status" field must be a JSON string
            var statusKind = root.GetProperty("status").ValueKind;
            var statusIsString = statusKind == JsonValueKind.String;

            // Also assert "previousStatus" field is a JSON string
            var previousStatusKind = root.GetProperty("previousStatus").ValueKind;
            var previousStatusIsString = previousStatusKind == JsonValueKind.String;

            return statusIsString.Label($"status ValueKind was {statusKind} (expected String) for enum value {status}")
                .And(previousStatusIsString.Label($"previousStatus ValueKind was {previousStatusKind} (expected String) for enum value {status}"));
        });
    }

    /// <summary>
    /// Property 1 (cross-check): The string value of the "status" field must equal
    /// the enum member name, not a numeric representation.
    /// </summary>
    [Property(MaxTest = 200)]
    public Property StatusField_StringValue_MatchesEnumName()
    {
        var gen = Gen.Elements(Enum.GetValues<ShipmentStatus>());
        var arb = Arb.From(gen);

        return Prop.ForAll(arb, status =>
        {
            var evt = new ShipmentStatusChanged
            {
                ShipmentId = Guid.NewGuid(),
                TrackingNumber = "SHP-TEST-0002",
                NewStatus = status,
                OldStatus = status,
                Reason = "test",
                Location = "Test Hub"
            };

            var json = JsonSerializer.Serialize(evt, JsonOptions);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var statusValue = root.GetProperty("status").GetString();
            var expectedValue = status.ToString();

            return (statusValue == expectedValue)
                .Label($"status string was '{statusValue}' but expected '{expectedValue}' for enum value {status}");
        });
    }
}
