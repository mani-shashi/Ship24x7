using FluentAssertions;
using MediatR;
using Moq;
using Ship24X7.Tracking.Application.Commands;
using Ship24X7.Tracking.Application.Handlers;
using Ship24X7.Tracking.Application.Interfaces;
using Ship24X7.Tracking.Domain.Entities;
using Ship24X7.Tracking.Domain.Enums;
using Ship24X7.Tracking.Domain.Events;
using Xunit;

namespace Ship24X7.Tracking.Tests.Handlers;

/// <summary>
/// Handler for processing CaptureDeliveryProofTests requests. Implements business logic and coordinates with repositories and services.
/// </summary>
public class CaptureDeliveryProofCommandHandlerTests
{
    private readonly Mock<IDeliveryProofRepository> _mockDeliveryProofRepository;
    private readonly Mock<ITrackingEventRepository> _mockTrackingEventRepository;
    private readonly Mock<IDocumentStorageService> _mockDocumentStorageService;
    private readonly Mock<IPublisher> _mockPublisher;
    private readonly CaptureDeliveryProofCommandHandler _sut;

    public CaptureDeliveryProofCommandHandlerTests()
    {
        _mockDeliveryProofRepository = new Mock<IDeliveryProofRepository>();
        _mockTrackingEventRepository = new Mock<ITrackingEventRepository>();
        _mockDocumentStorageService = new Mock<IDocumentStorageService>();
        _mockPublisher = new Mock<IPublisher>();

        _sut = new CaptureDeliveryProofCommandHandler(
            _mockDeliveryProofRepository.Object,
            _mockTrackingEventRepository.Object,
            _mockDocumentStorageService.Object,
            _mockPublisher.Object);
    }

    [Fact]
    public async Task Handle_WithShipmentInOutForDeliveryStatus_CapturesDeliveryProof()
    {
        // Arrange
        var command = CreateValidCommand();
        var latestEvent = CreateTrackingEvent(ShipmentStatus.OutForDelivery);

        _mockTrackingEventRepository
            .Setup(x => x.GetLatestByShipmentIdAsync(command.ShipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(latestEvent);

        _mockDocumentStorageService
            .Setup(x => x.UploadDocumentAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("uploaded-file-url");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ShipmentId.Should().Be(command.ShipmentId);
        result.TrackingNumber.Should().Be(command.TrackingNumber);
        result.ReceivedBy.Should().Be(command.ReceivedBy);
        result.Latitude.Should().Be(command.Latitude);
        result.Longitude.Should().Be(command.Longitude);

        _mockDeliveryProofRepository.Verify(
            x => x.AddAsync(It.IsAny<DeliveryProof>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockTrackingEventRepository.Verify(
            x => x.AddAsync(It.Is<TrackingEvent>(e => e.Status == ShipmentStatus.Delivered), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(ShipmentStatus.Draft)]
    [InlineData(ShipmentStatus.Booked)]
    [InlineData(ShipmentStatus.PaymentPending)]
    [InlineData(ShipmentStatus.Paid)]
    [InlineData(ShipmentStatus.PickedUp)]
    [InlineData(ShipmentStatus.InTransit)]
    [InlineData(ShipmentStatus.Delivered)]
    [InlineData(ShipmentStatus.Cancelled)]
    public async Task Handle_WithShipmentNotInOutForDeliveryStatus_ThrowsInvalidOperationException(ShipmentStatus status)
    {
        // Arrange
        var command = CreateValidCommand();
        var latestEvent = CreateTrackingEvent(status);

        _mockTrackingEventRepository
            .Setup(x => x.GetLatestByShipmentIdAsync(command.ShipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(latestEvent);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Shipment must be in OutForDelivery status to capture delivery proof");

        _mockDeliveryProofRepository.Verify(
            x => x.AddAsync(It.IsAny<DeliveryProof>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNoTrackingEvents_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = CreateValidCommand();

        _mockTrackingEventRepository
            .Setup(x => x.GetLatestByShipmentIdAsync(command.ShipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TrackingEvent?)null);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Shipment must be in OutForDelivery status to capture delivery proof");
    }

    [Theory]
    [InlineData(90, 180)]
    [InlineData(-90, -180)]
    [InlineData(0, 0)]
    [InlineData(45.5, -120.75)]
    public async Task Handle_WithValidGpsCoordinates_CreatesDeliveryProof(decimal latitude, decimal longitude)
    {
        // Arrange
        var command = CreateValidCommand();
        command.Latitude = latitude;
        command.Longitude = longitude;

        var latestEvent = CreateTrackingEvent(ShipmentStatus.OutForDelivery);

        _mockTrackingEventRepository
            .Setup(x => x.GetLatestByShipmentIdAsync(command.ShipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(latestEvent);

        _mockDocumentStorageService
            .Setup(x => x.UploadDocumentAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("uploaded-file-url");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Latitude.Should().Be(latitude);
        result.Longitude.Should().Be(longitude);
    }

    [Theory]
    [InlineData(90.1, 0)]
    [InlineData(-90.1, 0)]
    [InlineData(0, 180.1)]
    [InlineData(0, -180.1)]
    public async Task Handle_WithInvalidGpsCoordinates_ThrowsArgumentException(decimal latitude, decimal longitude)
    {
        // Arrange
        var command = CreateValidCommand();
        command.Latitude = latitude;
        command.Longitude = longitude;

        var latestEvent = CreateTrackingEvent(ShipmentStatus.OutForDelivery);

        _mockTrackingEventRepository
            .Setup(x => x.GetLatestByShipmentIdAsync(command.ShipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(latestEvent);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_UploadsSignatureAndPhotoImages()
    {
        // Arrange
        var command = CreateValidCommand();
        var latestEvent = CreateTrackingEvent(ShipmentStatus.OutForDelivery);

        _mockTrackingEventRepository
            .Setup(x => x.GetLatestByShipmentIdAsync(command.ShipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(latestEvent);

        _mockDocumentStorageService
            .Setup(x => x.UploadDocumentAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("uploaded-file-url");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _mockDocumentStorageService.Verify(
            x => x.UploadDocumentAsync(
                It.Is<string>(s => s.Contains("signature")),
                It.IsAny<byte[]>(),
                "image/png",
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockDocumentStorageService.Verify(
            x => x.UploadDocumentAsync(
                It.Is<string>(s => s.Contains("photo")),
                It.IsAny<byte[]>(),
                "image/jpeg",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_PublishesShipmentDeliveredEvent()
    {
        // Arrange
        var command = CreateValidCommand();
        var latestEvent = CreateTrackingEvent(ShipmentStatus.OutForDelivery);

        _mockTrackingEventRepository
            .Setup(x => x.GetLatestByShipmentIdAsync(command.ShipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(latestEvent);

        _mockDocumentStorageService
            .Setup(x => x.UploadDocumentAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("uploaded-file-url");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _mockPublisher.Verify(
            x => x.Publish(It.IsAny<ShipmentDelivered>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CreatesTrackingEventWithDeliveredStatus()
    {
        // Arrange
        var command = CreateValidCommand();
        var latestEvent = CreateTrackingEvent(ShipmentStatus.OutForDelivery);

        _mockTrackingEventRepository
            .Setup(x => x.GetLatestByShipmentIdAsync(command.ShipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(latestEvent);

        _mockDocumentStorageService
            .Setup(x => x.UploadDocumentAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("uploaded-file-url");

        TrackingEvent? capturedEvent = null;
        _mockTrackingEventRepository
            .Setup(x => x.AddAsync(It.IsAny<TrackingEvent>(), It.IsAny<CancellationToken>()))
            .Callback<TrackingEvent, CancellationToken>((e, ct) => capturedEvent = e)
            .ReturnsAsync((TrackingEvent?)null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent!.Status.Should().Be(ShipmentStatus.Delivered);
        capturedEvent.Description.Should().Contain(command.ReceivedBy);
        capturedEvent.IsException.Should().BeFalse();
    }

    private CaptureDeliveryProofCommand CreateValidCommand()
    {
        return new CaptureDeliveryProofCommand
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20260414001",
            ReceivedBy = "John Doe",
            DeliveryDate = DateTime.UtcNow.AddMinutes(-10),
            SignatureImageBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 }),
            PhotoProofBase64 = Convert.ToBase64String(new byte[] { 4, 5, 6 }),
            Latitude = 45.5m,
            Longitude = -120.75m,
            DeliveredBy = Guid.NewGuid(),
            Notes = "Delivered to front door"
        };
    }

    private TrackingEvent CreateTrackingEvent(ShipmentStatus status)
    {
        return new TrackingEvent
        {
            Id = Guid.NewGuid(),
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20260414001",
            Status = status,
            Description = $"Status: {status}",
            Location = "Test Location",
            EventTimestamp = DateTime.UtcNow,
            IsException = false,
            RecordedBy = "System",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };
    }
}
