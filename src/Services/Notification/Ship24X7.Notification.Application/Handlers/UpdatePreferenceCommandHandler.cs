using MediatR;
using Ship24X7.Notification.Application.Commands;
using Ship24X7.Notification.Application.Interfaces;
using Ship24X7.Notification.Domain.Entities;

namespace Ship24X7.Notification.Application.Handlers;

/// <summary>
/// Handles the UpdatePreferenceCommand by creating or updating user notification preferences.
/// Implements upsert logic: creates new preference record if none exists, updates existing record if found.
/// Updates all channel enablement flags (Email/SMS/Push/InApp) and event subscription settings.
/// Used by NotificationController when users modify their notification preferences through settings UI.
/// Ensures users can control which notifications they receive and through which channels.
/// </summary>
public class UpdatePreferenceCommandHandler : IRequestHandler<UpdatePreferenceCommand, Unit>
{
    private readonly INotificationPreferenceRepository _preferenceRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePreferenceCommandHandler"/> class.
    /// Injects preference repository for querying and persisting user preference data.
    /// </summary>
    /// <param name="preferenceRepository">Repository for creating and updating notification preferences.</param>
    public UpdatePreferenceCommandHandler(INotificationPreferenceRepository preferenceRepository)
    {
        _preferenceRepository = preferenceRepository;
    }

    /// <summary>
    /// Creates or updates user notification preferences based on whether preference record exists.
    /// Logic flow: 1) Queries database for existing preference record by user ID,
    /// 2) If not found (first-time setup): Creates new NotificationPreference entity with all settings from command,
    ///    sets CreatedAt timestamp and CreatedBy to user ID, persists to database via AddAsync,
    /// 3) If found (update existing): Updates all channel flags (Email/SMS/Push/InApp) and event subscription flags,
    ///    sets UpdatedAt timestamp and UpdatedBy to user ID, persists changes via UpdateAsync,
    /// 4) Returns Unit.Value indicating successful completion without returning data.
    /// Implements upsert pattern to handle both first-time setup and subsequent updates seamlessly.
    /// </summary>
    /// <param name="request">Update preference command containing user ID and all channel/event preference flags.</param>
    /// <param name="cancellationToken">Cancellation token to abort operation if request is cancelled.</param>
    /// <returns>
    /// Unit.Value indicating successful creation or update of preferences.
    /// No data returned as command is fire-and-forget operation.
    /// </returns>
    public async Task<Unit> Handle(UpdatePreferenceCommand request, CancellationToken cancellationToken)
    {
        // Query database for existing preference record - returns null if not found
        var preference = await _preferenceRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        
        if (preference == null)
        {
            // Create new preference record - first-time setup for user
            preference = new NotificationPreference
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                EmailEnabled = request.EmailEnabled,
                SmsEnabled = request.SmsEnabled,
                PushEnabled = request.PushEnabled,
                InAppEnabled = request.InAppEnabled,
                BookingConfirmationEnabled = request.BookingConfirmationEnabled,
                DeliveryConfirmationEnabled = request.DeliveryConfirmationEnabled,
                PaymentConfirmationEnabled = request.PaymentConfirmationEnabled,
                DelayAlertsEnabled = request.DelayAlertsEnabled,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.UserId
            };
            // Persist new preference record to database
            await _preferenceRepository.AddAsync(preference, cancellationToken);
        }
        else
        {
            // Update existing preference record - user modifying settings
            preference.EmailEnabled = request.EmailEnabled;
            preference.SmsEnabled = request.SmsEnabled;
            preference.PushEnabled = request.PushEnabled;
            preference.InAppEnabled = request.InAppEnabled;
            preference.BookingConfirmationEnabled = request.BookingConfirmationEnabled;
            preference.DeliveryConfirmationEnabled = request.DeliveryConfirmationEnabled;
            preference.PaymentConfirmationEnabled = request.PaymentConfirmationEnabled;
            preference.DelayAlertsEnabled = request.DelayAlertsEnabled;
            preference.UpdatedAt = DateTime.UtcNow;
            preference.UpdatedBy = request.UserId;
            
            // Persist updated preference record to database
            await _preferenceRepository.UpdateAsync(preference, cancellationToken);
        }

        return Unit.Value; // Indicate successful completion
    }
}
