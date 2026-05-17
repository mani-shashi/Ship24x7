using MediatR;
using Ship24X7.Tracking.Application.DTOs;

namespace Ship24X7.Tracking.Application.Commands;

/// <summary>
/// Command for generatecustomsdeclaration operation. Encapsulates request data and validation rules.
/// </summary>
public class GenerateCustomsDeclarationCommand : IRequest<DocumentResponse>
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
    /// Gets or sets the Sender country.
    /// </summary>
    public string SenderCountry { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Receiver name.
    /// </summary>
    public string ReceiverName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Receiver country.
    /// </summary>
    public string ReceiverCountry { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the declaredvalue.
    /// </summary>
    public decimal DeclaredValue { get; set; }
    /// <summary>
    /// Gets or sets the currency.
    /// </summary>
    public string Currency { get; set; } = "INR";
    public List<CustomsItemDto> Items { get; set; } = new();
}

/// <summary>
/// CustomsItemDto implementation. Provides functionality for the application.
/// </summary>
public class CustomsItemDto
{
    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the quantity.
    /// </summary>
    public int Quantity { get; set; }
    /// <summary>
    /// Gets or sets the weight.
    /// </summary>
    public decimal Weight { get; set; }
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public decimal Value { get; set; }
}
