using MediatR;
using Ship24X7.Notification.Application.DTOs;
using Ship24X7.Notification.Application.Interfaces;
using Ship24X7.Notification.Application.Queries;

namespace Ship24X7.Notification.Application.Handlers;

/// <summary>
/// Handles the GetUserPreferenceQuery by retrieving notification preferences for a specific user.
/// Queries database for user's preference record and maps entity to response DTO.
/// Returns null if user has no preference record (first-time user who hasn't set preferences yet).
/// Used by NotificationController to display user's current preference settings in UI.
/// </summary>
public class GetUserPreferenceQueryHandler : IRequestHandler<GetUserPreferenceQuery, NotificationPreferenceResponse?>
{
    private readonly INotificationPreferenceRepository _preferenceRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserPreferenceQueryHandler"/> class.
    /// Injects preference repository for querying user preference data from database.
    /// </summary>
    /// <param name="preferenceRepository">Repository for retrieving notification preferences by user ID.</param>
    public GetUserPreferenceQueryHandler(INotificationPreferenceRepository preferenceRepository)
    {
        _preferenceRepository = preferenceRepository;
    }

    /// <summary>
    /// Retrieves notification preferences for the specified user.
    /// Logic flow: 1) Queries database for preference record by user ID,
    /// 2) Returns null if no preference record exists (user hasn't set preferences yet),
    /// 3) Maps preference entity to NotificationPreferenceResponse DTO if found,
    /// 4) Includes all channel enablement flags (Email/SMS/Push/InApp) and event subscription settings,
    /// 5) Returns DTO for API response showing user's current preference configuration.
    /// </summary>
    /// <param name="request">Query containing user ID to retrieve preferences for.</param>
    /// <param name="cancellationToken">Cancellation token to abort query operation if request is cancelled.</param>
    /// <returns>
    /// NotificationPreferenceResponse DTO containing user's channel and event preferences if record exists.
    /// Null if user has no preference record (first-time user or preferences deleted).
    /// Controller returns 404 Not Found when null to indicate user needs to set up preferences.
    /// </returns>
    public async Task<NotificationPreferenceResponse?> Handle(GetUserPreferenceQuery request, CancellationToken cancellationToken)
    {
        // Query database for user's preference record - returns null if not found
        var preference = await _preferenceRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        // Return null if no preference record exists - controller will return 404
        if (preference == null)
        {
            return null;
        }

        // Map domain entity to response DTO - includes all channel and event preference flags
        return new NotificationPreferenceResponse
        {
            Id = preference.Id,
            UserId = preference.UserId,
            EmailEnabled = preference.EmailEnabled,
            SmsEnabled = preference.SmsEnabled,
            PushEnabled = preference.PushEnabled,
            InAppEnabled = preference.InAppEnabled,
            BookingConfirmationEnabled = preference.BookingConfirmationEnabled,
            DeliveryConfirmationEnabled = preference.DeliveryConfirmationEnabled,
            PaymentConfirmationEnabled = preference.PaymentConfirmationEnabled,
            DelayAlertsEnabled = preference.DelayAlertsEnabled
        };
    }
}
