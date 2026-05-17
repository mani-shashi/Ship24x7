using MediatR;
using Ship24X7.Auth.Application.DTOs;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Application.Queries;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Returns the user's preferences, or sensible defaults if none have been saved yet.
/// </summary>
public class GetPreferencesQueryHandler : IRequestHandler<GetPreferencesQuery, UserPreferencesDto>
{
    private readonly IUserPreferencesRepository _preferencesRepository;

    public GetPreferencesQueryHandler(IUserPreferencesRepository preferencesRepository)
    {
        _preferencesRepository = preferencesRepository;
    }

    public async Task<UserPreferencesDto> Handle(GetPreferencesQuery request, CancellationToken cancellationToken)
    {
        var prefs = await _preferencesRepository.GetByUserIdAsync(request.UserId);

        if (prefs == null)
            return new UserPreferencesDto(); // return defaults

        return new UserPreferencesDto
        {
            EmailNotifications = prefs.EmailNotifications,
            SmsNotifications = prefs.SmsNotifications,
            PushNotifications = prefs.PushNotifications,
            MarketingEmails = prefs.MarketingEmails,
            Theme = prefs.Theme,
            Language = prefs.Language
        };
    }
}
