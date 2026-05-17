using MediatR;
using Ship24X7.Payment.Application.DTOs;

namespace Ship24X7.Payment.Application.Commands;

/// <summary>
/// Command for initiating a refund for a captured payment order.
/// Encapsulates all required data for refund processing including payment order reference, amount, and reason.
/// Implements IRequest pattern from MediatR for CQRS architecture.
/// Returns RefundResponse containing refund ID and Razorpay refund ID for tracking.
/// Only accessible by Admin_User and System_Admin roles.
/// Refund amount must not exceed original payment amount and payment must be in Captured status.
/// </summary>
public class InitiateRefundCommand : IRequest<RefundResponse>
{
    /// <summary>
    /// Gets or sets the unique identifier of the payment order to be refunded.
    /// Payment order must exist and be in Captured status.
    /// Required field. Must be a valid payment order ID from the payment database.
    /// </summary>
    public Guid PaymentOrderId { get; set; }

    /// <summary>
    /// Gets or sets the refund amount in the payment order's currency.
    /// Must be positive, greater than zero, and not exceed the original payment amount.
    /// Partial refunds are supported (amount less than original payment).
    /// Full refunds use the original payment amount.
    /// Example: 250.00 for partial refund of ₹250 from ₹500 payment
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the reason for initiating the refund.
    /// Used for audit trail and customer communication.
    /// Should clearly explain why refund is being processed.
    /// Required field. Examples: "Customer cancellation", "Service not delivered", "Duplicate payment"
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique identifier of the admin user who initiated the refund.
    /// Used for audit trail and accountability.
    /// Must be a valid user ID with Admin_User or System_Admin role.
    /// Required field. Automatically set from authenticated user context.
    /// </summary>
    public Guid InitiatedBy { get; set; }
}
