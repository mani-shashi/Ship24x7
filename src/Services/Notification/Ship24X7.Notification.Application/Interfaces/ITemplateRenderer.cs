namespace Ship24X7.Notification.Application.Interfaces;

/// <summary>
/// ITemplateRenderer implementation. Provides functionality for the application.
/// </summary>
public interface ITemplateRenderer
{
    Task<string> RenderAsync(string template, Dictionary<string, string> placeholderData, CancellationToken cancellationToken = default);
}
