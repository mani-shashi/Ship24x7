namespace Ship24X7.Shipment.Domain.ValueObjects;

/// <summary>
/// Value object representing Weight in the domain model. Immutable type with value-based equality.
/// </summary>
public class Weight : IEquatable<Weight>
{
    /// <summary>
    /// Gets or sets the kilograms.
    /// </summary>
    public decimal Kilograms { get; }

    private Weight(decimal kilograms)
    {
        Kilograms = kilograms;
    }

    public static Weight Create(decimal kilograms)
    {
        if (kilograms < 0.01m)
            throw new ArgumentException("Weight must be at least 0.01 kg", nameof(kilograms));

        if (kilograms > 10000m)
            throw new ArgumentException("Weight cannot exceed 10,000 kg", nameof(kilograms));

        return new Weight(Math.Round(kilograms, 2));
    }

    public bool Equals(Weight? other)
    {
        if (other is null) return false;
        return Kilograms == other.Kilograms;
    }

    public override bool Equals(object? obj) => Equals(obj as Weight);
    public override int GetHashCode() => Kilograms.GetHashCode();
    public override string ToString() => $"{Kilograms:F2} kg";
}
