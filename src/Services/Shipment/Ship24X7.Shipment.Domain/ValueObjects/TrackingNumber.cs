namespace Ship24X7.Shipment.Domain.ValueObjects;

/// <summary>
/// Value object representing TrackingNumber in the domain model. Immutable type with value-based equality.
/// </summary>
public class TrackingNumber : IEquatable<TrackingNumber>
{
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public string Value { get; }

    private TrackingNumber(string value)
    {
        Value = value;
    }

    public static TrackingNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Tracking number cannot be empty", nameof(value));

        if (!value.StartsWith("SHIP24X7-"))
            throw new ArgumentException("Tracking number must start with SHIP24X7-", nameof(value));

        return new TrackingNumber(value);
    }

    public bool Equals(TrackingNumber? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as TrackingNumber);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
}
