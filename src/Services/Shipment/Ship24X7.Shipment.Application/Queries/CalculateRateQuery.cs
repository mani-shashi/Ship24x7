using MediatR;
using Ship24X7.Shipment.Application.DTOs;

namespace Ship24X7.Shipment.Application.Queries;

/// <summary>
/// Query for retrieving calculaterate data. Defines query parameters and result type.
/// </summary>
public class CalculateRateQuery : IRequest<List<RateCalculationResponse>>
{
    /// <summary>
    /// Gets or sets the actualweight.
    /// </summary>
    public decimal ActualWeight { get; set; }
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
    /// Gets or sets the Service type.
    /// </summary>
    public string? ServiceType { get; set; }
    /// <summary>
    /// Gets or sets the declaredvalue.
    /// </summary>
    public decimal? DeclaredValue { get; set; }
}
