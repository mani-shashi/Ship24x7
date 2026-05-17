using FluentAssertions;
using Moq;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Domain.Entities;
using Ship24X7.Shipment.Infrastructure.Services;
using Xunit;

namespace Ship24X7.Shipment.Tests.Services;

/// <summary>
/// RateCalculationTests service implementation. Provides ratecalculationtests functionality for the application.
/// </summary>
public class RateCalculationServiceTests
{
    private readonly Mock<IServiceRateRepository> _mockServiceRateRepository;
    private readonly RateCalculationService _sut;

    public RateCalculationServiceTests()
    {
        _mockServiceRateRepository = new Mock<IServiceRateRepository>();
        _sut = new RateCalculationService(_mockServiceRateRepository.Object);
    }

    [Fact]
    public async Task CalculateRatesAsync_WithActualWeightGreaterThanVolumetric_UsesActualWeight()
    {
        // Arrange
        var actualWeight = 50m; // 50 kg
        var length = 30m;
        var width = 20m;
        var height = 10m;
        // Volumetric weight = (30 * 20 * 10) / 5000 = 1.2 kg
        
        var serviceRate = CreateServiceRate("Domestic", 10m, 5m, 100m, 2);
        _mockServiceRateRepository
            .Setup(x => x.GetActiveRatesAsync(null))
            .ReturnsAsync(new List<ServiceRate> { serviceRate });

        // Act
        var result = await _sut.CalculateRatesAsync(actualWeight, length, width, height);

        // Assert
        result.Should().HaveCount(1);
        var rate = result.First();
        rate.ActualWeight.Should().Be(50m);
        rate.VolumetricWeight.Should().Be(1.2m);
        rate.ChargeableWeight.Should().Be(50m); // Max of 50 and 1.2
        rate.BaseRate.Should().Be(500m); // 50 * 10
        rate.FuelSurcharge.Should().Be(25m); // 500 * 0.05
        rate.TotalCost.Should().Be(525m); // 500 + 25
    }

    [Fact]
    public async Task CalculateRatesAsync_WithVolumetricWeightGreaterThanActual_UsesVolumetricWeight()
    {
        // Arrange
        var actualWeight = 5m; // 5 kg
        var length = 100m;
        var width = 100m;
        var height = 100m;
        // Volumetric weight = (100 * 100 * 100) / 5000 = 200 kg
        
        var serviceRate = CreateServiceRate("Domestic", 10m, 5m, 100m, 2);
        _mockServiceRateRepository
            .Setup(x => x.GetActiveRatesAsync(null))
            .ReturnsAsync(new List<ServiceRate> { serviceRate });

        // Act
        var result = await _sut.CalculateRatesAsync(actualWeight, length, width, height);

        // Assert
        result.Should().HaveCount(1);
        var rate = result.First();
        rate.ActualWeight.Should().Be(5m);
        rate.VolumetricWeight.Should().Be(200m);
        rate.ChargeableWeight.Should().Be(200m); // Max of 5 and 200
        rate.BaseRate.Should().Be(2000m); // 200 * 10
        rate.FuelSurcharge.Should().Be(100m); // 2000 * 0.05
        rate.TotalCost.Should().Be(2100m); // 2000 + 100
    }

    [Fact]
    public async Task CalculateRatesAsync_WithDeclaredValue_IncludesInsuranceCost()
    {
        // Arrange
        var actualWeight = 10m;
        var length = 30m;
        var width = 20m;
        var height = 10m;
        var declaredValue = 10000m; // INR 10,000
        
        var serviceRate = CreateServiceRate("Domestic", 10m, 5m, 100m, 2);
        _mockServiceRateRepository
            .Setup(x => x.GetActiveRatesAsync(null))
            .ReturnsAsync(new List<ServiceRate> { serviceRate });

        // Act
        var result = await _sut.CalculateRatesAsync(actualWeight, length, width, height, null, declaredValue);

        // Assert
        result.Should().HaveCount(1);
        var rate = result.First();
        rate.InsuranceCost.Should().Be(100m); // 10000 * 0.01
        rate.TotalCost.Should().Be(205m); // 100 (base) + 5 (fuel) + 100 (insurance)
    }

    [Fact]
    public async Task CalculateRatesAsync_WithLowCost_EnforcesMinimumCharge()
    {
        // Arrange
        var actualWeight = 0.5m; // 0.5 kg
        var length = 10m;
        var width = 10m;
        var height = 10m;
        
        var serviceRate = CreateServiceRate("Domestic", 10m, 5m, 100m, 2);
        _mockServiceRateRepository
            .Setup(x => x.GetActiveRatesAsync(null))
            .ReturnsAsync(new List<ServiceRate> { serviceRate });

        // Act
        var result = await _sut.CalculateRatesAsync(actualWeight, length, width, height);

        // Assert
        result.Should().HaveCount(1);
        var rate = result.First();
        rate.BaseRate.Should().Be(5m); // 0.5 * 10
        rate.FuelSurcharge.Should().Be(0.25m); // 5 * 0.05
        rate.TotalCost.Should().Be(100m); // Minimum charge enforced
    }

    [Fact]
    public async Task CalculateRatesAsync_WithMultipleServiceRates_ReturnsAllRatesSortedByPrice()
    {
        // Arrange
        var actualWeight = 10m;
        var length = 30m;
        var width = 20m;
        var height = 10m;
        
        var domesticRate = CreateServiceRate("Domestic", 10m, 5m, 100m, 3);
        var expressRate = CreateServiceRate("Express", 20m, 5m, 150m, 1);
        var freightRate = CreateServiceRate("Freight", 5m, 5m, 80m, 7);
        
        _mockServiceRateRepository
            .Setup(x => x.GetActiveRatesAsync(null))
            .ReturnsAsync(new List<ServiceRate> { domesticRate, expressRate, freightRate });

        // Act
        var result = await _sut.CalculateRatesAsync(actualWeight, length, width, height);

        // Assert
        result.Should().HaveCount(3);
        result[0].ServiceType.Should().Be("Freight"); // Cheapest
        result[1].ServiceType.Should().Be("Domestic");
        result[2].ServiceType.Should().Be("Express"); // Most expensive
    }

    [Fact]
    public async Task CalculateRatesAsync_WithSpecificServiceType_ReturnsOnlyMatchingRates()
    {
        // Arrange
        var actualWeight = 10m;
        var length = 30m;
        var width = 20m;
        var height = 10m;
        
        var expressRate = CreateServiceRate("Express", 20m, 5m, 150m, 1);
        
        _mockServiceRateRepository
            .Setup(x => x.GetActiveRatesAsync("Express"))
            .ReturnsAsync(new List<ServiceRate> { expressRate });

        // Act
        var result = await _sut.CalculateRatesAsync(actualWeight, length, width, height, "Express");

        // Assert
        result.Should().HaveCount(1);
        result[0].ServiceType.Should().Be("Express");
    }

    [Fact]
    public async Task CalculateRatesAsync_WithNoActiveRates_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockServiceRateRepository
            .Setup(x => x.GetActiveRatesAsync(null))
            .ReturnsAsync(new List<ServiceRate>());

        // Act
        var act = async () => await _sut.CalculateRatesAsync(10m, 30m, 20m, 10m);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No active service rates found");
    }

    [Theory]
    [InlineData(10, 30, 20, 10)]
    [InlineData(15, 40, 25, 15)]
    public async Task CalculateRatesAsync_WithSameInputs_ProducesDeterministicResults(
        decimal weight, decimal length, decimal width, decimal height)
    {
        // Arrange
        var serviceRate = CreateServiceRate("Domestic", 10m, 5m, 100m, 2);
        _mockServiceRateRepository
            .Setup(x => x.GetActiveRatesAsync(null))
            .ReturnsAsync(new List<ServiceRate> { serviceRate });

        // Act
        var result1 = await _sut.CalculateRatesAsync(weight, length, width, height);
        var result2 = await _sut.CalculateRatesAsync(weight, length, width, height);

        // Assert - Deterministic calculation property
        result1.First().TotalCost.Should().Be(result2.First().TotalCost);
        result1.First().ChargeableWeight.Should().Be(result2.First().ChargeableWeight);
        result1.First().VolumetricWeight.Should().Be(result2.First().VolumetricWeight);
    }

    [Fact]
    public async Task CalculateRatesAsync_CalculatesEstimatedDeliveryDate()
    {
        // Arrange
        var actualWeight = 10m;
        var length = 30m;
        var width = 20m;
        var height = 10m;
        var estimatedDays = 3;
        
        var serviceRate = CreateServiceRate("Domestic", 10m, 5m, 100m, estimatedDays);
        _mockServiceRateRepository
            .Setup(x => x.GetActiveRatesAsync(null))
            .ReturnsAsync(new List<ServiceRate> { serviceRate });

        var beforeCalculation = DateTime.UtcNow;

        // Act
        var result = await _sut.CalculateRatesAsync(actualWeight, length, width, height);

        // Assert
        var afterCalculation = DateTime.UtcNow;
        result.Should().HaveCount(1);
        var rate = result.First();
        rate.EstimatedDeliveryDays.Should().Be(estimatedDays);
        rate.EstimatedDeliveryDate.Should().BeAfter(beforeCalculation.AddDays(estimatedDays - 1));
        rate.EstimatedDeliveryDate.Should().BeBefore(afterCalculation.AddDays(estimatedDays + 1));
    }

    [Fact]
    public async Task CalculateRatesAsync_RoundsValuesToTwoDecimalPlaces()
    {
        // Arrange
        var actualWeight = 10.123m;
        var length = 30.456m;
        var width = 20.789m;
        var height = 10.111m;
        
        var serviceRate = CreateServiceRate("Domestic", 10.333m, 5.555m, 100m, 2);
        _mockServiceRateRepository
            .Setup(x => x.GetActiveRatesAsync(null))
            .ReturnsAsync(new List<ServiceRate> { serviceRate });

        // Act
        var result = await _sut.CalculateRatesAsync(actualWeight, length, width, height);

        // Assert
        result.Should().HaveCount(1);
        var rate = result.First();
        rate.ActualWeight.Should().Be(10.12m);
        rate.VolumetricWeight.Should().Be(12.73m); // (30.456 * 20.789 * 10.111) / 5000
        rate.ChargeableWeight.Should().Be(12.73m);
        rate.BaseRate.Should().Be(131.51m);
        rate.FuelSurcharge.Should().Be(7.31m);
        rate.TotalCost.Should().Be(138.82m);
    }

    private ServiceRate CreateServiceRate(
        string serviceType, 
        decimal baseRatePerKg, 
        decimal fuelSurchargePercent, 
        decimal minimumCharge,
        int estimatedDeliveryDays)
    {
        return new ServiceRate
        {
            Id = Guid.NewGuid(),
            ServiceType = serviceType,
            ServiceName = $"{serviceType} Service",
            BaseRatePerKg = baseRatePerKg,
            FuelSurchargePercent = fuelSurchargePercent,
            MinimumCharge = minimumCharge,
            EstimatedDeliveryDays = estimatedDeliveryDays,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };
    }
}
