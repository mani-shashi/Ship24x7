namespace Ship24X7.Tracking.Application.Interfaces;

/// <summary>
/// ILabelGeneration service implementation. Provides ilabelgeneration functionality for the application.
/// </summary>
public interface ILabelGenerationService
{
    Task<byte[]> GenerateShippingLabelAsync(
        string trackingNumber,
        string senderName,
        string senderAddress,
        string receiverName,
        string receiverAddress,
        string serviceType,
        decimal weight,
        CancellationToken cancellationToken = default);
}
