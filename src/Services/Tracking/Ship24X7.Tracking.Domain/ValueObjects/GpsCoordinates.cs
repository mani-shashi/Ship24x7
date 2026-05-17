namespace Ship24X7.Tracking.Domain.ValueObjects;

/// <summary>
/// Value object representing GpsCoordinates in the domain model. Immutable type with value-based equality.
/// </summary>
public class GpsCoordinates : IEquatable<GpsCoordinates>
{
    /// <summary>
    /// Gets or sets the latitude.
    /// </summary>
    public decimal Latitude { get; }
    /// <summary>
    /// Gets or sets the longitude.
    /// </summary>
    public decimal Longitude { get; }

    private GpsCoordinates(decimal latitude, decimal longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public static GpsCoordinates Create(decimal latitude, decimal longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));

        if (longitude < -180 || longitude > 180)
            throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));

        return new GpsCoordinates(latitude, longitude);
    }

    public bool Equals(GpsCoordinates? other)
    {
        if (other is null) return false;
        return Latitude == other.Latitude && Longitude == other.Longitude;
    }

    public override bool Equals(object? obj) => Equals(obj as GpsCoordinates);
    public override int GetHashCode() => HashCode.Combine(Latitude, Longitude);
    public override string ToString() => $"({Latitude:F6}, {Longitude:F6})";
}
