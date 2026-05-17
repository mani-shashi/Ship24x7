namespace Ship24X7.Notification.Application.Interfaces;

/// <summary>
/// IEmail service implementation. Provides iemail functionality for the application.
/// </summary>
public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
