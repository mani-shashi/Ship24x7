namespace Ship24X7.Auth.Application.DTOs;

/// <summary>
/// DTO for user notification and display preferences.
/// </summary>
public class UserPreferencesDto
{
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
    public bool PushNotifications { get; set; } = true;
    public bool MarketingEmails { get; set; } = false;
    public string Theme { get; set; } = "light";
    public string Language { get; set; } = "en-IN";
}
