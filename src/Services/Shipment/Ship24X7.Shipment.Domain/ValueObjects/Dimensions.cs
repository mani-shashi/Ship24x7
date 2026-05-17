namespace Ship24X7.Shipment.Domain.ValueObjects;

/// <summary>
/// Value object representing Dimensions in the domain model. Immutable type with value-based equality.
/// </summary>
public class Dimensions : IEquatable<Dimensions>
{
    /// <summary>
    /// Gets or sets the length.
    /// </summary>
    public decimal Length { get; }
    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    public decimal Width { get; }
    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    public decimal Height { get; }

    private Dimensions(decimal length, decimal width, decimal height)
    {
        Length = length;
        Width = width;
        Height = height;
    }

    public static Dimensions Create(decimal length, decimal width, decimal height)
    {
        ValidateDimension(length, nameof(length));
        ValidateDimension(width, nameof(width));
        ValidateDimension(height, nameof(height));

        return new Dimensions(
            Math.Round(length, 2),
            Math.Round(width, 2),
            Math.Round(height, 2)
        );
    }

    private static void ValidateDimension(decimal value, string paramName)
    {
        if (value < 1m)
            throw new ArgumentException($"Dimension must be at least 1 cm", paramName);

        if (value > 500m)
            throw new ArgumentException($"Dimension cannot exceed 500 cm", paramName);
    }

    public decimal CalculateVolume() => Length * Width * Height;

    public bool Equals(Dimensions? other)
    {
        if (other is null) return false;
        return Length == other.Length && Width == other.Width && Height == other.Height;
    }

    public override bool Equals(object? obj) => Equals(obj as Dimensions);
    public override int GetHashCode() => HashCode.Combine(Length, Width, Height);
    public override string ToString() => $"{Length:F2} x {Width:F2} x {Height:F2} cm";
}
