using Ship24X7.Shared.Domain;

namespace Ship24X7.Shipment.Domain.Events;

/// <summary>
/// Published when a shipment moves to OutForDelivery status.
/// Carries the raw delivery OTP so the Notification Service can send it
/// to the customer via SMS/email. The OTP is never stored in plaintext —
/// only this event carries it transiently.
/// </summary>
public class ShipmentOutForDelivery : BaseDomainEvent
{
    public Guid ShipmentId { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string DeliveryAgentId { get; set; } = string.Empty;

    /// <summary>
    /// Raw 6-digit OTP. Consumed once by Notification Service and never persisted.
    /// </summary>
    public string DeliveryOtp { get; set; } = string.Empty;

    public DateTime OtpExpiresAt { get; set; }
}
