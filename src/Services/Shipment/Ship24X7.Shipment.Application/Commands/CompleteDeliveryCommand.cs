using MediatR;

namespace Ship24X7.Shipment.Application.Commands;

/// <summary>
/// Transitions a shipment from OutForDelivery → Delivered.
/// Validates the 6-digit OTP provided by the delivery agent against the stored hash.
/// Supports supervisor override for edge cases (expired OTP, customer unavailable).
/// Supervisor override requires the caller to hold the Hub_User or Admin_User role
/// — enforced in the controller before dispatching this command.
/// </summary>
public class CompleteDeliveryCommand : IRequest<bool>
{
    /// <summary>Shipment to mark as delivered.</summary>
    public Guid ShipmentId { get; set; }

    /// <summary>
    /// The 6-digit OTP entered by the delivery agent as provided by the customer.
    /// Required unless SupervisorOverride is true.
    /// </summary>
    public string Otp { get; set; } = string.Empty;

    /// <summary>ID of the delivery agent or supervisor completing the delivery.</summary>
    public Guid OperatorId { get; set; }

    /// <summary>
    /// When true, skips OTP validation and marks delivered with an override note.
    /// Must only be set by callers that have verified the operator holds a supervisor role.
    /// </summary>
    public bool SupervisorOverride { get; set; }
}
