using FluentAssertions;
using Ship24X7.Tracking.Domain.ValueObjects;
using Xunit;

namespace Ship24X7.Tracking.Tests.ValueObjects;

/// <summary>
/// Value object representing GpsCoordinatesTests in the domain model. Immutable type with value-based equality.
/// </summary>
public class GpsCoordinatesTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(90, 180)]
    [InlineData(-90, -180)]
    [InlineData(45.5, 120.75)]
    [InlineData(-45.5, -120.75)]
    public void Create_WithValidCoordinates_ReturnsGpsCoordinates(decimal latitude, decimal longitude)
    {
        // Act
        var result = GpsCoordinates.Create(latitude, longitude);

        // Assert
        result.Should().NotBeNull();
        result.Latitude.Should().Be(latitude);
        result.Longitude.Should().Be(longitude);
    }

    [Theory]
    [InlineData(90.1, 0)]
    [InlineData(91, 0)]
    [InlineData(100, 0)]
    [InlineData(-90.1, 0)]
    [InlineData(-91, 0)]
    [InlineData(-100, 0)]
    public void Create_WithInvalidLatitude_ThrowsArgumentException(decimal latitude, decimal longitude)
    {
        // Act
        var act = () => GpsCoordinates.Create(latitude, longitude);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Latitude must be between -90 and 90*")
            .And.ParamName.Should().Be("latitude");
    }

    [Theory]
    [InlineData(0, 180.1)]
    [InlineData(0, 181)]
    [InlineData(0, 200)]
    [InlineData(0, -180.1)]
    [InlineData(0, -181)]
    [InlineData(0, -200)]
    public void Create_WithInvalidLongitude_ThrowsArgumentException(decimal latitude, decimal longitude)
    {
        // Act
        var act = () => GpsCoordinates.Create(latitude, longitude);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Longitude must be between -180 and 180*")
            .And.ParamName.Should().Be("longitude");
    }

    [Fact]
    public void Create_WithBoundaryLatitude_AcceptsExactBoundaries()
    {
        // Arrange & Act
        var northPole = GpsCoordinates.Create(90, 0);
        var southPole = GpsCoordinates.Create(-90, 0);

        // Assert
        northPole.Latitude.Should().Be(90);
        southPole.Latitude.Should().Be(-90);
    }

    [Fact]
    public void Create_WithBoundaryLongitude_AcceptsExactBoundaries()
    {
        // Arrange & Act
        var eastBoundary = GpsCoordinates.Create(0, 180);
        var westBoundary = GpsCoordinates.Create(0, -180);

        // Assert
        eastBoundary.Longitude.Should().Be(180);
        westBoundary.Longitude.Should().Be(-180);
    }

    [Fact]
    public void Equals_WithSameCoordinates_ReturnsTrue()
    {
        // Arrange
        var coords1 = GpsCoordinates.Create(45.5m, 120.75m);
        var coords2 = GpsCoordinates.Create(45.5m, 120.75m);

        // Act & Assert
        coords1.Equals(coords2).Should().BeTrue();
        (coords1 == coords2).Should().BeFalse(); // Reference equality
    }

    [Fact]
    public void Equals_WithDifferentCoordinates_ReturnsFalse()
    {
        // Arrange
        var coords1 = GpsCoordinates.Create(45.5m, 120.75m);
        var coords2 = GpsCoordinates.Create(45.6m, 120.75m);

        // Act & Assert
        coords1.Equals(coords2).Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsFormattedCoordinates()
    {
        // Arrange
        var coords = GpsCoordinates.Create(45.123456m, -120.654321m);

        // Act
        var result = coords.ToString();

        // Assert
        result.Should().Be("(45.123456, -120.654321)");
    }
}
