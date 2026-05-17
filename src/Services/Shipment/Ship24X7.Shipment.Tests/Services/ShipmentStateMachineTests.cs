using FluentAssertions;
using Ship24X7.Shipment.Domain.Enums;
using Ship24X7.Shipment.Infrastructure.Services;
using Xunit;

namespace Ship24X7.Shipment.Tests.Services;

/// <summary>
/// ShipmentStateMachineTests implementation. Provides functionality for the application.
/// </summary>
public class ShipmentStateMachineTests
{
    private readonly ShipmentStateMachine _sut;

    public ShipmentStateMachineTests()
    {
        _sut = new ShipmentStateMachine();
    }

    #region Draft Status Transitions

    [Theory]
    [InlineData(ShipmentStatus.Booked)]
    [InlineData(ShipmentStatus.Cancelled)]
    public void CanTransition_FromDraft_ToValidStatus_ReturnsTrue(ShipmentStatus newStatus)
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.Draft, newStatus);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Theory]
    [InlineData(ShipmentStatus.PaymentPending)]
    [InlineData(ShipmentStatus.Paid)]
    [InlineData(ShipmentStatus.PickedUp)]
    [InlineData(ShipmentStatus.InTransit)]
    [InlineData(ShipmentStatus.Delivered)]
    public void CanTransition_FromDraft_ToInvalidStatus_ReturnsFalse(ShipmentStatus newStatus)
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.Draft, newStatus);

        // Assert
        canTransition.Should().BeFalse();
    }

    #endregion

    #region Booked Status Transitions

    [Theory]
    [InlineData(ShipmentStatus.PaymentPending)]
    [InlineData(ShipmentStatus.Paid)]
    [InlineData(ShipmentStatus.Cancelled)]
    public void CanTransition_FromBooked_ToValidStatus_ReturnsTrue(ShipmentStatus newStatus)
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.Booked, newStatus);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Theory]
    [InlineData(ShipmentStatus.Draft)]
    [InlineData(ShipmentStatus.PickedUp)]
    [InlineData(ShipmentStatus.InTransit)]
    [InlineData(ShipmentStatus.Delivered)]
    public void CanTransition_FromBooked_ToInvalidStatus_ReturnsFalse(ShipmentStatus newStatus)
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.Booked, newStatus);

        // Assert
        canTransition.Should().BeFalse();
    }

    #endregion

    #region PaymentPending Status Transitions

    [Theory]
    [InlineData(ShipmentStatus.Paid)]
    [InlineData(ShipmentStatus.PaymentFailed)]
    [InlineData(ShipmentStatus.Cancelled)]
    public void CanTransition_FromPaymentPending_ToValidStatus_ReturnsTrue(ShipmentStatus newStatus)
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.PaymentPending, newStatus);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Theory]
    [InlineData(ShipmentStatus.Draft)]
    [InlineData(ShipmentStatus.Booked)]
    [InlineData(ShipmentStatus.PickedUp)]
    public void CanTransition_FromPaymentPending_ToInvalidStatus_ReturnsFalse(ShipmentStatus newStatus)
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.PaymentPending, newStatus);

        // Assert
        canTransition.Should().BeFalse();
    }

    #endregion

    #region Paid Status Transitions

    [Theory]
    [InlineData(ShipmentStatus.PickedUp)]
    [InlineData(ShipmentStatus.Cancelled)]
    public void CanTransition_FromPaid_ToValidStatus_ReturnsTrue(ShipmentStatus newStatus)
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.Paid, newStatus);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Theory]
    [InlineData(ShipmentStatus.Draft)]
    [InlineData(ShipmentStatus.Booked)]
    [InlineData(ShipmentStatus.PaymentPending)]
    [InlineData(ShipmentStatus.InTransit)]
    [InlineData(ShipmentStatus.Delivered)]
    public void CanTransition_FromPaid_ToInvalidStatus_ReturnsFalse(ShipmentStatus newStatus)
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.Paid, newStatus);

        // Assert
        canTransition.Should().BeFalse();
    }

    #endregion

    #region PickedUp Status Transitions

    [Theory]
    [InlineData(ShipmentStatus.InTransit)]
    [InlineData(ShipmentStatus.Delayed)]
    [InlineData(ShipmentStatus.Failed)]
    [InlineData(ShipmentStatus.Returned)]
    public void CanTransition_FromPickedUp_ToValidStatus_ReturnsTrue(ShipmentStatus newStatus)
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.PickedUp, newStatus);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Theory]
    [InlineData(ShipmentStatus.Draft)]
    [InlineData(ShipmentStatus.Booked)]
    [InlineData(ShipmentStatus.Paid)]
    [InlineData(ShipmentStatus.Cancelled)]
    [InlineData(ShipmentStatus.Delivered)]
    public void CanTransition_FromPickedUp_ToInvalidStatus_ReturnsFalse(ShipmentStatus newStatus)
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.PickedUp, newStatus);

        // Assert
        canTransition.Should().BeFalse();
    }

    #endregion

    #region InTransit Status Transitions

    [Theory]
    [InlineData(ShipmentStatus.OutForDelivery)]
    [InlineData(ShipmentStatus.Delayed)]
    [InlineData(ShipmentStatus.Failed)]
    [InlineData(ShipmentStatus.Returned)]
    public void CanTransition_FromInTransit_ToValidStatus_ReturnsTrue(ShipmentStatus newStatus)
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.InTransit, newStatus);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Theory]
    [InlineData(ShipmentStatus.Draft)]
    [InlineData(ShipmentStatus.Booked)]
    [InlineData(ShipmentStatus.Paid)]
    [InlineData(ShipmentStatus.PickedUp)]
    [InlineData(ShipmentStatus.Cancelled)]
    public void CanTransition_FromInTransit_ToInvalidStatus_ReturnsFalse(ShipmentStatus newStatus)
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.InTransit, newStatus);

        // Assert
        canTransition.Should().BeFalse();
    }

    #endregion

    #region OutForDelivery Status Transitions

    [Theory]
    [InlineData(ShipmentStatus.Delivered)]
    [InlineData(ShipmentStatus.Delayed)]
    [InlineData(ShipmentStatus.Failed)]
    [InlineData(ShipmentStatus.Returned)]
    public void CanTransition_FromOutForDelivery_ToValidStatus_ReturnsTrue(ShipmentStatus newStatus)
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.OutForDelivery, newStatus);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Theory]
    [InlineData(ShipmentStatus.Draft)]
    [InlineData(ShipmentStatus.Booked)]
    [InlineData(ShipmentStatus.Paid)]
    [InlineData(ShipmentStatus.PickedUp)]
    [InlineData(ShipmentStatus.InTransit)]
    [InlineData(ShipmentStatus.Cancelled)]
    public void CanTransition_FromOutForDelivery_ToInvalidStatus_ReturnsFalse(ShipmentStatus newStatus)
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.OutForDelivery, newStatus);

        // Assert
        canTransition.Should().BeFalse();
    }

    #endregion

    #region Delayed Status Transitions

    [Theory]
    [InlineData(ShipmentStatus.InTransit)]
    [InlineData(ShipmentStatus.OutForDelivery)]
    [InlineData(ShipmentStatus.Failed)]
    [InlineData(ShipmentStatus.Returned)]
    public void CanTransition_FromDelayed_ToValidStatus_ReturnsTrue(ShipmentStatus newStatus)
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.Delayed, newStatus);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Theory]
    [InlineData(ShipmentStatus.Draft)]
    [InlineData(ShipmentStatus.Booked)]
    [InlineData(ShipmentStatus.Paid)]
    [InlineData(ShipmentStatus.PickedUp)]
    [InlineData(ShipmentStatus.Delivered)]
    [InlineData(ShipmentStatus.Cancelled)]
    public void CanTransition_FromDelayed_ToInvalidStatus_ReturnsFalse(ShipmentStatus newStatus)
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.Delayed, newStatus);

        // Assert
        canTransition.Should().BeFalse();
    }

    #endregion

    #region PaymentFailed Status Transitions

    [Theory]
    [InlineData(ShipmentStatus.PaymentPending)]
    [InlineData(ShipmentStatus.Cancelled)]
    public void CanTransition_FromPaymentFailed_ToValidStatus_ReturnsTrue(ShipmentStatus newStatus)
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.PaymentFailed, newStatus);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Theory]
    [InlineData(ShipmentStatus.Draft)]
    [InlineData(ShipmentStatus.Booked)]
    [InlineData(ShipmentStatus.Paid)]
    [InlineData(ShipmentStatus.PickedUp)]
    [InlineData(ShipmentStatus.Delivered)]
    public void CanTransition_FromPaymentFailed_ToInvalidStatus_ReturnsFalse(ShipmentStatus newStatus)
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.PaymentFailed, newStatus);

        // Assert
        canTransition.Should().BeFalse();
    }

    #endregion

    #region Failed Status Transitions

    [Fact]
    public void CanTransition_FromFailed_ToReturned_ReturnsTrue()
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.Failed, ShipmentStatus.Returned);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Theory]
    [InlineData(ShipmentStatus.Draft)]
    [InlineData(ShipmentStatus.Booked)]
    [InlineData(ShipmentStatus.Paid)]
    [InlineData(ShipmentStatus.PickedUp)]
    [InlineData(ShipmentStatus.InTransit)]
    [InlineData(ShipmentStatus.Delivered)]
    [InlineData(ShipmentStatus.Cancelled)]
    public void CanTransition_FromFailed_ToInvalidStatus_ReturnsFalse(ShipmentStatus newStatus)
    {
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.Failed, newStatus);

        // Assert
        canTransition.Should().BeFalse();
    }

    #endregion

    #region Terminal States

    [Theory]
    [InlineData(ShipmentStatus.Draft)]
    [InlineData(ShipmentStatus.Booked)]
    [InlineData(ShipmentStatus.Paid)]
    [InlineData(ShipmentStatus.PickedUp)]
    [InlineData(ShipmentStatus.InTransit)]
    [InlineData(ShipmentStatus.OutForDelivery)]
    [InlineData(ShipmentStatus.Delayed)]
    [InlineData(ShipmentStatus.Failed)]
    [InlineData(ShipmentStatus.Returned)]
    [InlineData(ShipmentStatus.Cancelled)]
    [InlineData(ShipmentStatus.PaymentPending)]
    [InlineData(ShipmentStatus.PaymentFailed)]
    public void CanTransition_FromDelivered_ToAnyStatus_ReturnsFalse(ShipmentStatus newStatus)
    {
        // Delivered is a terminal state - no transitions allowed
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.Delivered, newStatus);

        // Assert
        canTransition.Should().BeFalse();
    }

    [Theory]
    [InlineData(ShipmentStatus.Draft)]
    [InlineData(ShipmentStatus.Booked)]
    [InlineData(ShipmentStatus.Paid)]
    [InlineData(ShipmentStatus.PickedUp)]
    [InlineData(ShipmentStatus.InTransit)]
    [InlineData(ShipmentStatus.OutForDelivery)]
    [InlineData(ShipmentStatus.Delivered)]
    [InlineData(ShipmentStatus.Delayed)]
    [InlineData(ShipmentStatus.Failed)]
    [InlineData(ShipmentStatus.Returned)]
    [InlineData(ShipmentStatus.PaymentPending)]
    [InlineData(ShipmentStatus.PaymentFailed)]
    public void CanTransition_FromCancelled_ToAnyStatus_ReturnsFalse(ShipmentStatus newStatus)
    {
        // Cancelled is a terminal state - no transitions allowed
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.Cancelled, newStatus);

        // Assert
        canTransition.Should().BeFalse();
    }

    [Theory]
    [InlineData(ShipmentStatus.Draft)]
    [InlineData(ShipmentStatus.Booked)]
    [InlineData(ShipmentStatus.Paid)]
    [InlineData(ShipmentStatus.PickedUp)]
    [InlineData(ShipmentStatus.InTransit)]
    [InlineData(ShipmentStatus.OutForDelivery)]
    [InlineData(ShipmentStatus.Delivered)]
    [InlineData(ShipmentStatus.Delayed)]
    [InlineData(ShipmentStatus.Failed)]
    [InlineData(ShipmentStatus.Cancelled)]
    [InlineData(ShipmentStatus.PaymentPending)]
    [InlineData(ShipmentStatus.PaymentFailed)]
    public void CanTransition_FromReturned_ToAnyStatus_ReturnsFalse(ShipmentStatus newStatus)
    {
        // Returned is a terminal state - no transitions allowed
        // Act
        var canTransition = _sut.CanTransition(ShipmentStatus.Returned, newStatus);

        // Assert
        canTransition.Should().BeFalse();
    }

    #endregion

    #region GetValidTransitions Tests

    [Fact]
    public void GetValidTransitions_FromDraft_ReturnsCorrectTransitions()
    {
        // Act
        var validTransitions = _sut.GetValidTransitions(ShipmentStatus.Draft);

        // Assert
        validTransitions.Should().HaveCount(2);
        validTransitions.Should().Contain(ShipmentStatus.Booked);
        validTransitions.Should().Contain(ShipmentStatus.Cancelled);
    }

    [Fact]
    public void GetValidTransitions_FromBooked_ReturnsCorrectTransitions()
    {
        // Act
        var validTransitions = _sut.GetValidTransitions(ShipmentStatus.Booked);

        // Assert
        validTransitions.Should().HaveCount(3);
        validTransitions.Should().Contain(ShipmentStatus.PaymentPending);
        validTransitions.Should().Contain(ShipmentStatus.Paid);
        validTransitions.Should().Contain(ShipmentStatus.Cancelled);
    }

    [Fact]
    public void GetValidTransitions_FromInTransit_ReturnsCorrectTransitions()
    {
        // Act
        var validTransitions = _sut.GetValidTransitions(ShipmentStatus.InTransit);

        // Assert
        validTransitions.Should().HaveCount(4);
        validTransitions.Should().Contain(ShipmentStatus.OutForDelivery);
        validTransitions.Should().Contain(ShipmentStatus.Delayed);
        validTransitions.Should().Contain(ShipmentStatus.Failed);
        validTransitions.Should().Contain(ShipmentStatus.Returned);
    }

    [Fact]
    public void GetValidTransitions_FromDelivered_ReturnsEmptyList()
    {
        // Act
        var validTransitions = _sut.GetValidTransitions(ShipmentStatus.Delivered);

        // Assert
        validTransitions.Should().BeEmpty();
    }

    [Fact]
    public void GetValidTransitions_FromCancelled_ReturnsEmptyList()
    {
        // Act
        var validTransitions = _sut.GetValidTransitions(ShipmentStatus.Cancelled);

        // Assert
        validTransitions.Should().BeEmpty();
    }

    [Fact]
    public void GetValidTransitions_FromReturned_ReturnsEmptyList()
    {
        // Act
        var validTransitions = _sut.GetValidTransitions(ShipmentStatus.Returned);

        // Assert
        validTransitions.Should().BeEmpty();
    }

    #endregion

    #region Workflow Scenario Tests

    [Fact]
    public void HappyPath_DraftToDelivered_AllTransitionsValid()
    {
        // This test validates the happy path workflow
        var workflow = new[]
        {
            (ShipmentStatus.Draft, ShipmentStatus.Booked),
            (ShipmentStatus.Booked, ShipmentStatus.Paid),
            (ShipmentStatus.Paid, ShipmentStatus.PickedUp),
            (ShipmentStatus.PickedUp, ShipmentStatus.InTransit),
            (ShipmentStatus.InTransit, ShipmentStatus.OutForDelivery),
            (ShipmentStatus.OutForDelivery, ShipmentStatus.Delivered)
        };

        foreach (var (currentStatus, nextStatus) in workflow)
        {
            var canTransition = _sut.CanTransition(currentStatus, nextStatus);
            canTransition.Should().BeTrue(
                $"Should be able to transition from {currentStatus} to {nextStatus}");
        }
    }

    [Fact]
    public void PaymentFlow_WithPaymentPending_AllTransitionsValid()
    {
        // This test validates the payment pending workflow
        var workflow = new[]
        {
            (ShipmentStatus.Draft, ShipmentStatus.Booked),
            (ShipmentStatus.Booked, ShipmentStatus.PaymentPending),
            (ShipmentStatus.PaymentPending, ShipmentStatus.Paid),
            (ShipmentStatus.Paid, ShipmentStatus.PickedUp)
        };

        foreach (var (currentStatus, nextStatus) in workflow)
        {
            var canTransition = _sut.CanTransition(currentStatus, nextStatus);
            canTransition.Should().BeTrue(
                $"Should be able to transition from {currentStatus} to {nextStatus}");
        }
    }

    [Fact]
    public void ExceptionFlow_WithDelayAndRecovery_AllTransitionsValid()
    {
        // This test validates the delay and recovery workflow
        var workflow = new[]
        {
            (ShipmentStatus.InTransit, ShipmentStatus.Delayed),
            (ShipmentStatus.Delayed, ShipmentStatus.InTransit),
            (ShipmentStatus.InTransit, ShipmentStatus.OutForDelivery),
            (ShipmentStatus.OutForDelivery, ShipmentStatus.Delivered)
        };

        foreach (var (currentStatus, nextStatus) in workflow)
        {
            var canTransition = _sut.CanTransition(currentStatus, nextStatus);
            canTransition.Should().BeTrue(
                $"Should be able to transition from {currentStatus} to {nextStatus}");
        }
    }

    [Fact]
    public void FailureFlow_WithReturn_AllTransitionsValid()
    {
        // This test validates the failure and return workflow
        var workflow = new[]
        {
            (ShipmentStatus.PickedUp, ShipmentStatus.InTransit),
            (ShipmentStatus.InTransit, ShipmentStatus.Failed),
            (ShipmentStatus.Failed, ShipmentStatus.Returned)
        };

        foreach (var (currentStatus, nextStatus) in workflow)
        {
            var canTransition = _sut.CanTransition(currentStatus, nextStatus);
            canTransition.Should().BeTrue(
                $"Should be able to transition from {currentStatus} to {nextStatus}");
        }
    }

    #endregion
}
