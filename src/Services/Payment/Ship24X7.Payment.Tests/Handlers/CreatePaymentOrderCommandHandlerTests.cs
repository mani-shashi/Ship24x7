using FluentAssertions;
using Moq;
using Ship24X7.Payment.Application.Commands;
using Ship24X7.Payment.Application.Handlers;
using Ship24X7.Payment.Application.Interfaces;
using Ship24X7.Payment.Domain.Entities;

namespace Ship24X7.Payment.Tests.Handlers;

/// <summary>
/// Tests for payment order creation with Razorpay API and idempotency handling
/// **Validates: Requirements 17.2, 17.11**
/// </summary>
public class CreatePaymentOrderCommandHandlerTests
{
    private readonly Mock<IPaymentOrderRepository> _paymentOrderRepositoryMock;
    private readonly Mock<IRazorpayService> _razorpayServiceMock;
    private readonly CreatePaymentOrderCommandHandler _sut;

    public CreatePaymentOrderCommandHandlerTests()
    {
        _paymentOrderRepositoryMock = new Mock<IPaymentOrderRepository>();
        _razorpayServiceMock = new Mock<IRazorpayService>();

        _sut = new CreatePaymentOrderCommandHandler(
            _paymentOrderRepositoryMock.Object,
            _razorpayServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithNewPaymentOrder_CreatesRazorpayOrderAndReturnsResponse()
    {
        // Arrange
        var command = new CreatePaymentOrderCommand
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20260414001",
            Amount = 1500.00m,
            Currency = "INR",
            IdempotencyKey = "idempotency_key_123"
        };

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdempotencyKeyAsync(command.IdempotencyKey))
            .ReturnsAsync((PaymentOrder?)null);
        _paymentOrderRepositoryMock.Setup(x => x.GetByShipmentIdAsync(command.ShipmentId))
            .ReturnsAsync((PaymentOrder?)null);
        _razorpayServiceMock.Setup(x => x.CreateOrderAsync(
                command.Amount,
                command.Currency,
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync("order_MHkLZjQqvXZKqp");
        _razorpayServiceMock.Setup(x => x.GetKeyId())
            .Returns("rzp_test_1234567890");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RazorpayOrderId.Should().Be("order_MHkLZjQqvXZKqp");
        result.RazorpayKeyId.Should().Be("rzp_test_1234567890");
        result.Amount.Should().Be(1500.00m);
        result.Currency.Should().Be("INR");
    }

    [Fact]
    public async Task Handle_WithNewPaymentOrder_StoresPaymentOrderInDatabase()
    {
        // Arrange
        var command = new CreatePaymentOrderCommand
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20260414001",
            Amount = 1500.00m,
            Currency = "INR",
            IdempotencyKey = "idempotency_key_123"
        };

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdempotencyKeyAsync(command.IdempotencyKey))
            .ReturnsAsync((PaymentOrder?)null);
        _paymentOrderRepositoryMock.Setup(x => x.GetByShipmentIdAsync(command.ShipmentId))
            .ReturnsAsync((PaymentOrder?)null);
        _razorpayServiceMock.Setup(x => x.CreateOrderAsync(
                It.IsAny<decimal>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync("order_MHkLZjQqvXZKqp");
        _razorpayServiceMock.Setup(x => x.GetKeyId())
            .Returns("rzp_test_1234567890");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _paymentOrderRepositoryMock.Verify(x => x.AddAsync(It.Is<PaymentOrder>(po =>
            po.ShipmentId == command.ShipmentId &&
            po.TrackingNumber == command.TrackingNumber &&
            po.RazorpayOrderId == "order_MHkLZjQqvXZKqp" &&
            po.Amount == command.Amount &&
            po.Currency == command.Currency &&
            po.Status == PaymentStatus.Pending &&
            po.IdempotencyKey == command.IdempotencyKey
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingIdempotencyKey_ReturnsExistingOrderWithoutCreatingNew()
    {
        // Arrange
        var command = new CreatePaymentOrderCommand
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20260414001",
            Amount = 1500.00m,
            Currency = "INR",
            IdempotencyKey = "idempotency_key_123"
        };

        var existingOrder = new PaymentOrder
        {
            Id = Guid.NewGuid(),
            ShipmentId = command.ShipmentId,
            TrackingNumber = command.TrackingNumber,
            RazorpayOrderId = "order_EXISTING123",
            Amount = command.Amount,
            Currency = command.Currency,
            Status = PaymentStatus.Pending,
            IdempotencyKey = command.IdempotencyKey,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.Empty
        };

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdempotencyKeyAsync(command.IdempotencyKey))
            .ReturnsAsync(existingOrder);
        _razorpayServiceMock.Setup(x => x.GetKeyId())
            .Returns("rzp_test_1234567890");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PaymentOrderId.Should().Be(existingOrder.Id);
        result.RazorpayOrderId.Should().Be("order_EXISTING123");
        _razorpayServiceMock.Verify(x => x.CreateOrderAsync(
            It.IsAny<decimal>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>()), Times.Never);
        _paymentOrderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PaymentOrder>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExistingPendingPaymentForShipment_ReturnsExistingOrder()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var command = new CreatePaymentOrderCommand
        {
            ShipmentId = shipmentId,
            TrackingNumber = "SHIP24X7-20260414001",
            Amount = 1500.00m,
            Currency = "INR",
            IdempotencyKey = "idempotency_key_new"
        };

        var existingOrder = new PaymentOrder
        {
            Id = Guid.NewGuid(),
            ShipmentId = shipmentId,
            TrackingNumber = command.TrackingNumber,
            RazorpayOrderId = "order_EXISTING456",
            Amount = command.Amount,
            Currency = command.Currency,
            Status = PaymentStatus.Pending,
            IdempotencyKey = "idempotency_key_old",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.Empty
        };

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdempotencyKeyAsync(command.IdempotencyKey))
            .ReturnsAsync((PaymentOrder?)null);
        _paymentOrderRepositoryMock.Setup(x => x.GetByShipmentIdAsync(shipmentId))
            .ReturnsAsync(existingOrder);
        _razorpayServiceMock.Setup(x => x.GetKeyId())
            .Returns("rzp_test_1234567890");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PaymentOrderId.Should().Be(existingOrder.Id);
        result.RazorpayOrderId.Should().Be("order_EXISTING456");
        _razorpayServiceMock.Verify(x => x.CreateOrderAsync(
            It.IsAny<decimal>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExistingCapturedPaymentForShipment_ReturnsExistingOrder()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var command = new CreatePaymentOrderCommand
        {
            ShipmentId = shipmentId,
            TrackingNumber = "SHIP24X7-20260414001",
            Amount = 1500.00m,
            Currency = "INR",
            IdempotencyKey = "idempotency_key_new"
        };

        var existingOrder = new PaymentOrder
        {
            Id = Guid.NewGuid(),
            ShipmentId = shipmentId,
            TrackingNumber = command.TrackingNumber,
            RazorpayOrderId = "order_CAPTURED789",
            Amount = command.Amount,
            Currency = command.Currency,
            Status = PaymentStatus.Captured,
            IdempotencyKey = "idempotency_key_old",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.Empty
        };

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdempotencyKeyAsync(command.IdempotencyKey))
            .ReturnsAsync((PaymentOrder?)null);
        _paymentOrderRepositoryMock.Setup(x => x.GetByShipmentIdAsync(shipmentId))
            .ReturnsAsync(existingOrder);
        _razorpayServiceMock.Setup(x => x.GetKeyId())
            .Returns("rzp_test_1234567890");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PaymentOrderId.Should().Be(existingOrder.Id);
        result.RazorpayOrderId.Should().Be("order_CAPTURED789");
        _razorpayServiceMock.Verify(x => x.CreateOrderAsync(
            It.IsAny<decimal>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExistingFailedPaymentForShipment_CreatesNewOrder()
    {
        // Arrange
        var shipmentId = Guid.NewGuid();
        var command = new CreatePaymentOrderCommand
        {
            ShipmentId = shipmentId,
            TrackingNumber = "SHIP24X7-20260414001",
            Amount = 1500.00m,
            Currency = "INR",
            IdempotencyKey = "idempotency_key_new"
        };

        var existingFailedOrder = new PaymentOrder
        {
            Id = Guid.NewGuid(),
            ShipmentId = shipmentId,
            TrackingNumber = command.TrackingNumber,
            RazorpayOrderId = "order_FAILED999",
            Amount = command.Amount,
            Currency = command.Currency,
            Status = PaymentStatus.Failed,
            IdempotencyKey = "idempotency_key_old",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.Empty
        };

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdempotencyKeyAsync(command.IdempotencyKey))
            .ReturnsAsync((PaymentOrder?)null);
        _paymentOrderRepositoryMock.Setup(x => x.GetByShipmentIdAsync(shipmentId))
            .ReturnsAsync(existingFailedOrder);
        _razorpayServiceMock.Setup(x => x.CreateOrderAsync(
                It.IsAny<decimal>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync("order_NEW123");
        _razorpayServiceMock.Setup(x => x.GetKeyId())
            .Returns("rzp_test_1234567890");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RazorpayOrderId.Should().Be("order_NEW123");
        _razorpayServiceMock.Verify(x => x.CreateOrderAsync(
            It.IsAny<decimal>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>()), Times.Once);
        _paymentOrderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PaymentOrder>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CallsRazorpayWithCorrectParameters()
    {
        // Arrange
        var command = new CreatePaymentOrderCommand
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20260414001",
            Amount = 1500.00m,
            Currency = "INR",
            IdempotencyKey = "idempotency_key_123"
        };

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdempotencyKeyAsync(command.IdempotencyKey))
            .ReturnsAsync((PaymentOrder?)null);
        _paymentOrderRepositoryMock.Setup(x => x.GetByShipmentIdAsync(command.ShipmentId))
            .ReturnsAsync((PaymentOrder?)null);
        _razorpayServiceMock.Setup(x => x.CreateOrderAsync(
                It.IsAny<decimal>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync("order_MHkLZjQqvXZKqp");
        _razorpayServiceMock.Setup(x => x.GetKeyId())
            .Returns("rzp_test_1234567890");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _razorpayServiceMock.Verify(x => x.CreateOrderAsync(
            1500.00m,
            "INR",
            "SHIP_SHIP24X7-20260414001",
            It.Is<Dictionary<string, string>>(notes =>
                notes["shipment_id"] == command.ShipmentId.ToString() &&
                notes["tracking_number"] == command.TrackingNumber
            )), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRazorpayServiceFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new CreatePaymentOrderCommand
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20260414001",
            Amount = 1500.00m,
            Currency = "INR",
            IdempotencyKey = "idempotency_key_123"
        };

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdempotencyKeyAsync(command.IdempotencyKey))
            .ReturnsAsync((PaymentOrder?)null);
        _paymentOrderRepositoryMock.Setup(x => x.GetByShipmentIdAsync(command.ShipmentId))
            .ReturnsAsync((PaymentOrder?)null);
        _razorpayServiceMock.Setup(x => x.CreateOrderAsync(
                It.IsAny<decimal>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
            .ThrowsAsync(new HttpRequestException("Service unavailable"));

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to create Razorpay order. Service may be unavailable.");
        _paymentOrderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PaymentOrder>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SetsPaymentOrderStatusToPending()
    {
        // Arrange
        var command = new CreatePaymentOrderCommand
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20260414001",
            Amount = 1500.00m,
            Currency = "INR",
            IdempotencyKey = "idempotency_key_123"
        };

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdempotencyKeyAsync(command.IdempotencyKey))
            .ReturnsAsync((PaymentOrder?)null);
        _paymentOrderRepositoryMock.Setup(x => x.GetByShipmentIdAsync(command.ShipmentId))
            .ReturnsAsync((PaymentOrder?)null);
        _razorpayServiceMock.Setup(x => x.CreateOrderAsync(
                It.IsAny<decimal>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync("order_MHkLZjQqvXZKqp");
        _razorpayServiceMock.Setup(x => x.GetKeyId())
            .Returns("rzp_test_1234567890");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _paymentOrderRepositoryMock.Verify(x => x.AddAsync(It.Is<PaymentOrder>(po =>
            po.Status == PaymentStatus.Pending
        )), Times.Once);
    }
}
