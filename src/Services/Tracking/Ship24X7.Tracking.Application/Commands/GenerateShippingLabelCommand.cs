using MediatR;
using Ship24X7.Tracking.Application.DTOs;

namespace Ship24X7.Tracking.Application.Commands;

/// <summary>
/// Command for generateshippinglabel operation. Encapsulates request data and validation rules.
/// </summary>
public class GenerateShippingLabelCommand : IRequest<DocumentResponse>
{
    /// <summary>
    /// Gets or sets the shipmentId.
    /// </summary>
    public Guid ShipmentId { get; set; }
    /// <summary>
    /// Gets or sets the trackingNumber.
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Sender name.
    /// </summary>
    public string SenderName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Sender address.
    /// </summary>
    public string SenderAddress { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Receiver name.
    /// </summary>
    public string ReceiverName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Receiver address.
    /// </summary>
    public string ReceiverAddress { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Service type.
    /// </summary>
    public string ServiceType { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the weight.
    /// </summary>
    public decimal Weight { get; set; }
}
