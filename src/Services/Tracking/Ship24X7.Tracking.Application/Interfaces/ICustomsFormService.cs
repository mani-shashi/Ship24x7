using Ship24X7.Tracking.Application.Commands;

namespace Ship24X7.Tracking.Application.Interfaces;

/// <summary>
/// ICustomsForm service implementation. Provides icustomsform functionality for the application.
/// </summary>
public interface ICustomsFormService
{
    Task<byte[]> GenerateCustomsDeclarationAsync(
        string trackingNumber,
        string senderName,
        string senderCountry,
        string receiverName,
        string receiverCountry,
        decimal declaredValue,
        string currency,
        List<CustomsItemDto> items,
        CancellationToken cancellationToken = default);
}
