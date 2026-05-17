using Ship24X7.Tracking.Domain.Enums;

namespace Ship24X7.Tracking.Infrastructure.Messaging;

/// <summary>
/// Maps incoming status strings from the event bus to the Tracking service's local
/// <see cref="ShipmentStatus"/> enum.
///
/// The Tracking service owns its own copy of the enum and must never reference the
/// Shipment service's Domain or Infrastructure project. This mapper is the single
/// place where the string-to-enum conversion happens, making it easy to unit-test
/// in isolation.
/// </summary>
public static class StatusMapper
{
    /// <summary>
    /// Attempts to parse <paramref name="rawStatus"/> as a <see cref="ShipmentStatus"/> value
    /// (case-insensitive).
    /// </summary>
    /// <param name="rawStatus">The status string received from the event bus.</param>
    /// <returns>
    /// The matching <see cref="ShipmentStatus"/> when the string is a valid enum name;
    /// <see langword="null"/> when the string is unrecognised.
    /// </returns>
    public static ShipmentStatus? Parse(string? rawStatus)
    {
        if (string.IsNullOrWhiteSpace(rawStatus))
            return null;

        return Enum.TryParse<ShipmentStatus>(rawStatus, ignoreCase: true, out var status)
            ? status
            : null;
    }
}
