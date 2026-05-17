using FluentAssertions;
using Ship24X7.Shipment.Domain.ValueObjects;
using Xunit;

namespace Ship24X7.Shipment.Tests.ValueObjects;

/// <summary>
/// Value object representing DimensionsTests in the domain model. Immutable type with value-based equality.
/// </summary>
public class DimensionsTests
{
    [Fact]
    public void Create_WithValidDimensions_CreatesDimensionsObject()
    {
        // Arrange
        var length = 100m;
        var width = 50m;
        var height = 30m;

        // Act
        var dimensions = Dimensions.Create(length, width, height);

        // Assert
        dimensions.Length.Should().Be(100m);
        dimensions.Width.Should().Be(50m);
        dimensions.Height.Should().Be(30m);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithLengthLessThanMinimum_ThrowsArgumentException(decimal length)
    {
        // Act
        var act = () => Dimensions.Create(length, 50m, 30m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Dimension must be at least 1 cm*");
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithWidthLessThanMinimum_ThrowsArgumentException(decimal width)
    {
        // Act
        var act = () => Dimensions.Create(100m, width, 30m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Dimension must be at least 1 cm*");
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithHeightLessThanMinimum_ThrowsArgumentException(decimal height)
    {
        // Act
        var act = () => Dimensions.Create(100m, 50m, height);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Dimension must be at least 1 cm*");
    }

    [Theory]
    [InlineData(501)]
    [InlineData(1000)]
    public void Create_WithLengthExceedingMaximum_ThrowsArgumentException(decimal length)
    {
        // Act
        var act = () => Dimensions.Create(length, 50m, 30m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Dimension cannot exceed 500 cm*");
    }

    [Theory]
    [InlineData(501)]
    [InlineData(1000)]
    public void Create_WithWidthExceedingMaximum_ThrowsArgumentException(decimal width)
    {
        // Act
        var act = () => Dimensions.Create(100m, width, 30m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Dimension cannot exceed 500 cm*");
    }

    [Theory]
    [InlineData(501)]
    [InlineData(1000)]
    public void Create_WithHeightExceedingMaximum_ThrowsArgumentException(decimal height)
    {
        // Act
        var act = () => Dimensions.Create(100m, 50m, height);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Dimension cannot exceed 500 cm*");
    }

    [Fact]
    public void Create_RoundsDimensionsToTwoDecimalPlaces()
    {
        // Arrange
        var length = 100.12345m;
        var width = 50.98765m;
        var height = 30.55555m;

        // Act
        var dimensions = Dimensions.Create(length, width, height);

        // Assert
        dimensions.Length.Should().Be(100.12m);
        dimensions.Width.Should().Be(50.99m);
        dimensions.Height.Should().Be(30.56m);
    }

    [Fact]
    public void CalculateVolume_ReturnsCorrectVolume()
    {
        // Arrange
        var dimensions = Dimensions.Create(100m, 50m, 30m);

        // Act
        var volume = dimensions.CalculateVolume();

        // Assert
        volume.Should().Be(150000m); // 100 * 50 * 30
    }

    [Theory]
    [InlineData(100, 50, 30, 150000)]
    [InlineData(10, 10, 10, 1000)]
    [InlineData(200, 100, 50, 1000000)]
    public void CalculateVolume_WithVariousDimensions_ReturnsCorrectVolume(
        decimal length, decimal width, decimal height, decimal expectedVolume)
    {
        // Arrange
        var dimensions = Dimensions.Create(length, width, height);

        // Act
        var volume = dimensions.CalculateVolume();

        // Assert
        volume.Should().Be(expectedVolume);
    }

    [Fact]
    public void Equals_WithSameDimensions_ReturnsTrue()
    {
        // Arrange
        var dimensions1 = Dimensions.Create(100m, 50m, 30m);
        var dimensions2 = Dimensions.Create(100m, 50m, 30m);

        // Act & Assert
        dimensions1.Equals(dimensions2).Should().BeTrue();
        (dimensions1 == dimensions2).Should().BeFalse(); // Different instances
    }

    [Fact]
    public void Equals_WithDifferentDimensions_ReturnsFalse()
    {
        // Arrange
        var dimensions1 = Dimensions.Create(100m, 50m, 30m);
        var dimensions2 = Dimensions.Create(100m, 50m, 31m);

        // Act & Assert
        dimensions1.Equals(dimensions2).Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        var dimensions = Dimensions.Create(100m, 50m, 30m);

        // Act
        var result = dimensions.ToString();

        // Assert
        result.Should().Be("100.00 x 50.00 x 30.00 cm");
    }

    [Fact]
    public void GetHashCode_WithSameDimensions_ReturnsSameHashCode()
    {
        // Arrange
        var dimensions1 = Dimensions.Create(100m, 50m, 30m);
        var dimensions2 = Dimensions.Create(100m, 50m, 30m);

        // Act & Assert
        dimensions1.GetHashCode().Should().Be(dimensions2.GetHashCode());
    }
}
