using MediatR;

namespace Ship24X7.Shipment.Application.Commands;

/// <summary>
/// Transitions a shipment from InTransit/Delayed → OutForDelivery.
/// Assigns a delivery agent and generates a 6-digit OTP.
/// The OTP is sent to the customer via the Notification Service (via RabbitMQ event).
/// The raw OTP is returned in the response so the caller can confirm it was dispatched.
/// </summary>
public class MarkOutForDeliveryCommand : IRequest<MarkOutForDeliveryResult>
{
    /// <summary>Shipment to mark as out for delivery.</summary>
    public Guid ShipmentId { get; set; }

    /// <summary>ID or name of the delivery agent assigned for last-mile delivery.</summary>
    public string DeliveryAgentId { get; set; } = string.Empty;

    /// <summary>Hub the shipment is departing from for last-mile delivery.</summary>
    public Guid HubId { get; set; }

    /// <summary>ID of the operator performing the assignment.</summary>
    public Guid OperatorId { get; set; }
}

public class MarkOutForDeliveryResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// The raw OTP that was generated and sent to the customer.
    /// Exposed here for operational confirmation only — never log or store this.
    /// </summary>
    public string RawOtp { get; set; } = string.Empty;
}
