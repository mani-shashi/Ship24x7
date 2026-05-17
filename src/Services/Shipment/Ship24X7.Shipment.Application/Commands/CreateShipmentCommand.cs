using MediatR;
using Ship24X7.Shipment.Application.DTOs;

namespace Ship24X7.Shipment.Application.Commands;

/// <summary>
/// Command for createshipment operation. Encapsulates request data and validation rules.
/// </summary>
public class CreateShipmentCommand : IRequest<ShipmentResponse>
{
    /// <summary>
    /// Gets or sets the customerId.
    /// </summary>
    public Guid CustomerId { get; set; }
    /// <summary>
    /// Gets or sets the Idempotency key.
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;
    
    // Sender Address
    /// <summary>
    /// Gets or sets the SenderContact name.
    /// </summary>
    public string SenderContactName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the SenderContact phone.
    /// </summary>
    public string SenderContactPhone { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the SenderContact email.
    /// </summary>
    public string SenderContactEmail { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Sender addressLine1.
    /// </summary>
    public string SenderAddressLine1 { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Sender addressLine2.
    /// </summary>
    public string SenderAddressLine2 { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public string SenderCity { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the state or province.
    /// </summary>
    public string SenderState { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the SenderPostal code.
    /// </summary>
    public string SenderPostalCode { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Sender country.
    /// </summary>
    public string SenderCountry { get; set; } = string.Empty;
    
    // Receiver Address
    /// <summary>
    /// Gets or sets the ReceiverContact name.
    /// </summary>
    public string ReceiverContactName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the ReceiverContact phone.
    /// </summary>
    public string ReceiverContactPhone { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the ReceiverContact email.
    /// </summary>
    public string ReceiverContactEmail { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Receiver addressLine1.
    /// </summary>
    public string ReceiverAddressLine1 { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Receiver addressLine2.
    /// </summary>
    public string ReceiverAddressLine2 { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public string ReceiverCity { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the state or province.
    /// </summary>
    public string ReceiverState { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the ReceiverPostal code.
    /// </summary>
    public string ReceiverPostalCode { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the Receiver country.
    /// </summary>
    public string ReceiverCountry { get; set; } = string.Empty;
    
    // Shipment Items
    public List<ShipmentItemDto> Items { get; set; } = new();
    
    // Service
    /// <summary>
    /// Gets or sets the servicerateid.
    /// </summary>
    public Guid ServiceRateId { get; set; }
    /// <summary>
    /// Gets or sets the declaredvalue.
    /// </summary>
    public decimal? DeclaredValue { get; set; }
    /// <summary>
    /// Gets or sets the isfragile.
    /// </summary>
    public bool IsFragile { get; set; }
    /// <summary>
    /// Gets or sets the requiresrefrigeration.
    /// </summary>
    public bool RequiresRefrigeration { get; set; }
}

/// <summary>
/// ShipmentItemDto implementation. Provides functionality for the application.
/// </summary>
public class ShipmentItemDto
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
    /// Gets or sets the length.
    /// </summary>
    public decimal Length { get; set; }
    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    public decimal Width { get; set; }
    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    public decimal Height { get; set; }
    /// <summary>
    /// Gets or sets the Package type.
    /// </summary>
    public string PackageType { get; set; } = string.Empty;
}
