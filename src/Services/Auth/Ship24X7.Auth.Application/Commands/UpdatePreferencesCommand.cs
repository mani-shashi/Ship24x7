using MediatR;
using Ship24X7.Auth.Application.DTOs;

namespace Ship24X7.Auth.Application.Commands;

/// <summary>
/// Command to create or update the authenticated user's notification and display preferences.
/// Upserts the record — creates it on first save, updates on subsequent saves.
/// </summary>
public class UpdatePreferencesCommand : IRequest<UserPreferencesDto>
{
    public Guid UserId { get; set; }
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
    public bool PushNotifications { get; set; } = true;
    public bool MarketingEmails { get; set; } = false;
    public string Theme { get; set; } = "light";
    public string Language { get; set; } = "en-IN";
}
