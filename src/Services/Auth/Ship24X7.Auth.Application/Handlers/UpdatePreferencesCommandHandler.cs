using MediatR;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.DTOs;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Upserts the user's notification and display preferences.
/// Creates the record on first save, updates on subsequent saves.
/// </summary>
public class UpdatePreferencesCommandHandler : IRequestHandler<UpdatePreferencesCommand, UserPreferencesDto>
{
    private readonly IUserPreferencesRepository _preferencesRepository;

    public UpdatePreferencesCommandHandler(IUserPreferencesRepository preferencesRepository)
    {
        _preferencesRepository = preferencesRepository;
    }

    public async Task<UserPreferencesDto> Handle(UpdatePreferencesCommand request, CancellationToken cancellationToken)
    {
        var existing = await _preferencesRepository.GetByUserIdAsync(request.UserId);

        if (existing == null)
        {
            existing = new UserPreferences
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                EmailNotifications = request.EmailNotifications,
                SmsNotifications = request.SmsNotifications,
                PushNotifications = request.PushNotifications,
                MarketingEmails = request.MarketingEmails,
                Theme = request.Theme,
                Language = request.Language,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.UserId
            };

            await _preferencesRepository.AddAsync(existing);
        }
        else
        {
            existing.EmailNotifications = request.EmailNotifications;
            existing.SmsNotifications = request.SmsNotifications;
            existing.PushNotifications = request.PushNotifications;
            existing.MarketingEmails = request.MarketingEmails;
            existing.Theme = request.Theme;
            existing.Language = request.Language;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = request.UserId;

            await _preferencesRepository.UpdateAsync(existing);
        }

        return new UserPreferencesDto
        {
            EmailNotifications = existing.EmailNotifications,
            SmsNotifications = existing.SmsNotifications,
            PushNotifications = existing.PushNotifications,
            MarketingEmails = existing.MarketingEmails,
            Theme = existing.Theme,
            Language = existing.Language
        };
    }
}
