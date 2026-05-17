namespace Ship24X7.Shipment.Application.Interfaces;

/// <summary>
/// ITrackingNumberGenerator implementation. Provides functionality for the application.
/// </summary>
public interface ITrackingNumberGenerator
{
    Task<string> GenerateAsync();
}
