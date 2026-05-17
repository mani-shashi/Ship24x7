namespace Ship24X7.Shipment.Domain.ValueObjects;

/// <summary>
/// Value object representing GeoCoordinates in the domain model. Immutable type with value-based equality.
/// </summary>
public class GeoCoordinates : IEquatable<GeoCoordinates>
{
    /// <summary>
    /// Gets or sets the latitude.
    /// </summary>
    public decimal Latitude { get; }
    /// <summary>
    /// Gets or sets the longitude.
    /// </summary>
    public decimal Longitude { get; }

    private GeoCoordinates(decimal latitude, decimal longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public static GeoCoordinates Create(decimal latitude, decimal longitude)
    {
        if (latitude < -90m || latitude > 90m)
            throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));

        if (longitude < -180m || longitude > 180m)
            throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));

        return new GeoCoordinates(latitude, longitude);
    }

    public bool Equals(GeoCoordinates? other)
    {
        if (other is null) return false;
        return Latitude == other.Latitude && Longitude == other.Longitude;
    }

    public override bool Equals(object? obj) => Equals(obj as GeoCoordinates);
    public override int GetHashCode() => HashCode.Combine(Latitude, Longitude);
    public override string ToString() => $"{Latitude:F6}, {Longitude:F6}";
}
