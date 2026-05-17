namespace Ship24X7.Notification.Application.Interfaces;

/// <summary>
/// ISms service implementation. Provides isms functionality for the application.
/// </summary>
public interface ISmsService
{
    Task<bool> SendSmsAsync(string to, string message, CancellationToken cancellationToken = default);
}
