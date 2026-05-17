using FluentAssertions;
using Moq;
using Ship24X7.Payment.Application.Commands;
using Ship24X7.Payment.Application.Handlers;
using Ship24X7.Payment.Application.Interfaces;
using Ship24X7.Payment.Domain.Entities;

namespace Ship24X7.Payment.Tests.Handlers;

/// <summary>
/// Tests for refund initiation logic
/// **Validates: Requirements 17.2, 17.11**
/// </summary>
public class InitiateRefundCommandHandlerTests
{
    private readonly Mock<IPaymentOrderRepository> _paymentOrderRepositoryMock;
    private readonly Mock<IPaymentRefundRepository> _refundRepositoryMock;
    private readonly Mock<IRazorpayService> _razorpayServiceMock;
    private readonly InitiateRefundCommandHandler _sut;

    public InitiateRefundCommandHandlerTests()
    {
        _paymentOrderRepositoryMock = new Mock<IPaymentOrderRepository>();
        _refundRepositoryMock = new Mock<IPaymentRefundRepository>();
        _razorpayServiceMock = new Mock<IRazorpayService>();

        _sut = new InitiateRefundCommandHandler(
            _paymentOrderRepositoryMock.Object,
            _refundRepositoryMock.Object,
            _razorpayServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCapturedPayment_InitiatesRefundSuccessfully()
    {
        // Arrange
        var paymentOrderId = Guid.NewGuid();
        var initiatedBy = Guid.NewGuid();
        var command = new InitiateRefundCommand
        {
            PaymentOrderId = paymentOrderId,
            Amount = 1500.00m,
            Reason = "Customer requested cancellation",
            InitiatedBy = initiatedBy
        };

        var paymentOrder = CreateCapturedPaymentOrder(paymentOrderId);

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdAsync(paymentOrderId))
            .ReturnsAsync(paymentOrder);
        _razorpayServiceMock.Setup(x => x.InitiateRefundAsync(
                paymentOrder.RazorpayPaymentId!,
                command.Amount,
                command.Reason))
            .ReturnsAsync("rfnd_MHkLZjQqvXZKqp");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RazorpayRefundId.Should().Be("rfnd_MHkLZjQqvXZKqp");
        result.Status.Should().Be("Initiated");
        result.Amount.Should().Be(1500.00m);
        result.Currency.Should().Be("INR");
    }

    [Fact]
    public async Task Handle_WithValidRefund_StoresRefundInDatabase()
    {
        // Arrange
        var paymentOrderId = Guid.NewGuid();
        var initiatedBy = Guid.NewGuid();
        var command = new InitiateRefundCommand
        {
            PaymentOrderId = paymentOrderId,
            Amount = 1500.00m,
            Reason = "Customer requested cancellation",
            InitiatedBy = initiatedBy
        };

        var paymentOrder = CreateCapturedPaymentOrder(paymentOrderId);

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdAsync(paymentOrderId))
            .ReturnsAsync(paymentOrder);
        _razorpayServiceMock.Setup(x => x.InitiateRefundAsync(
                It.IsAny<string>(),
                It.IsAny<decimal>(),
                It.IsAny<string>()))
            .ReturnsAsync("rfnd_MHkLZjQqvXZKqp");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _refundRepositoryMock.Verify(x => x.AddAsync(It.Is<PaymentRefund>(r =>
            r.PaymentOrderId == paymentOrderId &&
            r.RazorpayRefundId == "rfnd_MHkLZjQqvXZKqp" &&
            r.Amount == 1500.00m &&
            r.Currency == "INR" &&
            r.Status == RefundStatus.Initiated &&
            r.Reason == "Customer requested cancellation" &&
            r.InitiatedBy == initiatedBy
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentPaymentOrder_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new InitiateRefundCommand
        {
            PaymentOrderId = Guid.NewGuid(),
            Amount = 1500.00m,
            Reason = "Customer requested cancellation",
            InitiatedBy = Guid.NewGuid()
        };

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdAsync(command.PaymentOrderId))
            .ReturnsAsync((PaymentOrder?)null);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Payment order not found");
        _razorpayServiceMock.Verify(x => x.InitiateRefundAsync(
            It.IsAny<string>(),
            It.IsAny<decimal>(),
            It.IsAny<string>()), Times.Never);
        _refundRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PaymentRefund>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithPendingPaymentOrder_ThrowsInvalidOperationException()
    {
        // Arrange
        var paymentOrderId = Guid.NewGuid();
        var command = new InitiateRefundCommand
        {
            PaymentOrderId = paymentOrderId,
            Amount = 1500.00m,
            Reason = "Customer requested cancellation",
            InitiatedBy = Guid.NewGuid()
        };

        var paymentOrder = CreateCapturedPaymentOrder(paymentOrderId);
        paymentOrder.Status = PaymentStatus.Pending;

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdAsync(paymentOrderId))
            .ReturnsAsync(paymentOrder);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Payment order must be in Captured status to initiate refund");
        _razorpayServiceMock.Verify(x => x.InitiateRefundAsync(
            It.IsAny<string>(),
            It.IsAny<decimal>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithFailedPaymentOrder_ThrowsInvalidOperationException()
    {
        // Arrange
        var paymentOrderId = Guid.NewGuid();
        var command = new InitiateRefundCommand
        {
            PaymentOrderId = paymentOrderId,
            Amount = 1500.00m,
            Reason = "Customer requested cancellation",
            InitiatedBy = Guid.NewGuid()
        };

        var paymentOrder = CreateCapturedPaymentOrder(paymentOrderId);
        paymentOrder.Status = PaymentStatus.Failed;

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdAsync(paymentOrderId))
            .ReturnsAsync(paymentOrder);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Payment order must be in Captured status to initiate refund");
    }

    [Fact]
    public async Task Handle_WithZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var paymentOrderId = Guid.NewGuid();
        var command = new InitiateRefundCommand
        {
            PaymentOrderId = paymentOrderId,
            Amount = 0m,
            Reason = "Customer requested cancellation",
            InitiatedBy = Guid.NewGuid()
        };

        var paymentOrder = CreateCapturedPaymentOrder(paymentOrderId);

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdAsync(paymentOrderId))
            .ReturnsAsync(paymentOrder);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid refund amount");
        _razorpayServiceMock.Verify(x => x.InitiateRefundAsync(
            It.IsAny<string>(),
            It.IsAny<decimal>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var paymentOrderId = Guid.NewGuid();
        var command = new InitiateRefundCommand
        {
            PaymentOrderId = paymentOrderId,
            Amount = -100m,
            Reason = "Customer requested cancellation",
            InitiatedBy = Guid.NewGuid()
        };

        var paymentOrder = CreateCapturedPaymentOrder(paymentOrderId);

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdAsync(paymentOrderId))
            .ReturnsAsync(paymentOrder);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid refund amount");
    }

    [Fact]
    public async Task Handle_WithAmountExceedingPaymentAmount_ThrowsArgumentException()
    {
        // Arrange
        var paymentOrderId = Guid.NewGuid();
        var command = new InitiateRefundCommand
        {
            PaymentOrderId = paymentOrderId,
            Amount = 2000.00m, // Payment order has 1500.00
            Reason = "Customer requested cancellation",
            InitiatedBy = Guid.NewGuid()
        };

        var paymentOrder = CreateCapturedPaymentOrder(paymentOrderId);

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdAsync(paymentOrderId))
            .ReturnsAsync(paymentOrder);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid refund amount");
        _razorpayServiceMock.Verify(x => x.InitiateRefundAsync(
            It.IsAny<string>(),
            It.IsAny<decimal>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithPartialRefund_InitiatesRefundSuccessfully()
    {
        // Arrange
        var paymentOrderId = Guid.NewGuid();
        var command = new InitiateRefundCommand
        {
            PaymentOrderId = paymentOrderId,
            Amount = 750.00m, // Partial refund
            Reason = "Partial cancellation",
            InitiatedBy = Guid.NewGuid()
        };

        var paymentOrder = CreateCapturedPaymentOrder(paymentOrderId);

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdAsync(paymentOrderId))
            .ReturnsAsync(paymentOrder);
        _razorpayServiceMock.Setup(x => x.InitiateRefundAsync(
                It.IsAny<string>(),
                It.IsAny<decimal>(),
                It.IsAny<string>()))
            .ReturnsAsync("rfnd_PARTIAL123");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(750.00m);
        _razorpayServiceMock.Verify(x => x.InitiateRefundAsync(
            paymentOrder.RazorpayPaymentId!,
            750.00m,
            "Partial cancellation"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRazorpayServiceFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var paymentOrderId = Guid.NewGuid();
        var command = new InitiateRefundCommand
        {
            PaymentOrderId = paymentOrderId,
            Amount = 1500.00m,
            Reason = "Customer requested cancellation",
            InitiatedBy = Guid.NewGuid()
        };

        var paymentOrder = CreateCapturedPaymentOrder(paymentOrderId);

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdAsync(paymentOrderId))
            .ReturnsAsync(paymentOrder);
        _razorpayServiceMock.Setup(x => x.InitiateRefundAsync(
                It.IsAny<string>(),
                It.IsAny<decimal>(),
                It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("Razorpay API error"));

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Failed to initiate refund with Razorpay");
        _refundRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PaymentRefund>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CallsRazorpayWithCorrectPaymentId()
    {
        // Arrange
        var paymentOrderId = Guid.NewGuid();
        var command = new InitiateRefundCommand
        {
            PaymentOrderId = paymentOrderId,
            Amount = 1500.00m,
            Reason = "Customer requested cancellation",
            InitiatedBy = Guid.NewGuid()
        };

        var paymentOrder = CreateCapturedPaymentOrder(paymentOrderId);

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdAsync(paymentOrderId))
            .ReturnsAsync(paymentOrder);
        _razorpayServiceMock.Setup(x => x.InitiateRefundAsync(
                It.IsAny<string>(),
                It.IsAny<decimal>(),
                It.IsAny<string>()))
            .ReturnsAsync("rfnd_MHkLZjQqvXZKqp");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _razorpayServiceMock.Verify(x => x.InitiateRefundAsync(
            "pay_MHkLZjQqvXZKqp",
            1500.00m,
            "Customer requested cancellation"), Times.Once);
    }

    [Fact]
    public async Task Handle_SetsRefundStatusToInitiated()
    {
        // Arrange
        var paymentOrderId = Guid.NewGuid();
        var command = new InitiateRefundCommand
        {
            PaymentOrderId = paymentOrderId,
            Amount = 1500.00m,
            Reason = "Customer requested cancellation",
            InitiatedBy = Guid.NewGuid()
        };

        var paymentOrder = CreateCapturedPaymentOrder(paymentOrderId);

        _paymentOrderRepositoryMock.Setup(x => x.GetByIdAsync(paymentOrderId))
            .ReturnsAsync(paymentOrder);
        _razorpayServiceMock.Setup(x => x.InitiateRefundAsync(
                It.IsAny<string>(),
                It.IsAny<decimal>(),
                It.IsAny<string>()))
            .ReturnsAsync("rfnd_MHkLZjQqvXZKqp");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _refundRepositoryMock.Verify(x => x.AddAsync(It.Is<PaymentRefund>(r =>
            r.Status == RefundStatus.Initiated
        )), Times.Once);
    }

    private static PaymentOrder CreateCapturedPaymentOrder(Guid paymentOrderId)
    {
        return new PaymentOrder
        {
            Id = paymentOrderId,
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20260414001",
            RazorpayOrderId = "order_MHkLZjQqvXZKqp",
            RazorpayPaymentId = "pay_MHkLZjQqvXZKqp",
            Amount = 1500.00m,
            Currency = "INR",
            Status = PaymentStatus.Captured,
            IdempotencyKey = "idempotency_key_123",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.Empty
        };
    }
}
