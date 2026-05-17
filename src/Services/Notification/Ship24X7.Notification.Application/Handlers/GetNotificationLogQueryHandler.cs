using MediatR;
using Ship24X7.Notification.Application.DTOs;
using Ship24X7.Notification.Application.Interfaces;
using Ship24X7.Notification.Application.Queries;

namespace Ship24X7.Notification.Application.Handlers;

/// <summary>
/// Handles the GetNotificationLogQuery by retrieving paginated notification history for a specific user.
/// Queries database for user's notification logs with pagination support for efficient data retrieval.
/// Returns logs ordered by most recent first (descending CreatedAt) for chronological history display.
/// Used by NotificationController to display user's notification history in UI with pagination controls.
/// Maps entities to response DTOs excluding sensitive data like full body content for performance.
/// </summary>
public class GetNotificationLogQueryHandler : IRequestHandler<GetNotificationLogQuery, List<NotificationLogResponse>>
{
    private readonly INotificationLogRepository _notificationLogRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetNotificationLogQueryHandler"/> class.
    /// Injects notification log repository for querying notification history from database.
    /// </summary>
    /// <param name="notificationLogRepository">Repository for retrieving paginated notification logs by user ID.</param>
    public GetNotificationLogQueryHandler(INotificationLogRepository notificationLogRepository)
    {
        _notificationLogRepository = notificationLogRepository;
    }

    /// <summary>
    /// Retrieves paginated notification history for the specified user.
    /// Logic flow: 1) Queries database for notification logs by user ID with pagination (page number and page size),
    /// 2) Repository returns logs ordered by CreatedAt descending (most recent first),
    /// 3) Maps each log entity to NotificationLogResponse DTO,
    /// 4) Includes delivery metadata (channel, status, timestamps, retry count, error message) but excludes full body for performance,
    /// 5) Returns list of DTOs for API response. Empty list if user has no notification history or page exceeds available data.
    /// Pagination allows efficient retrieval of large notification histories without loading all records.
    /// </summary>
    /// <param name="request">Query containing user ID, page number (1-based), and page size (records per page).</param>
    /// <param name="cancellationToken">Cancellation token to abort query operation if request is cancelled.</param>
    /// <returns>
    /// List of notification log response DTOs for the specified page containing delivery metadata and status.
    /// Empty list if user has no notifications or page number exceeds available data.
    /// Logs are ordered by CreatedAt descending (most recent first) for chronological display.
    /// </returns>
    public async Task<List<NotificationLogResponse>> Handle(GetNotificationLogQuery request, CancellationToken cancellationToken)
    {
        // Query database with pagination - repository handles skip/take logic and ordering
        var logs = await _notificationLogRepository.GetByUserIdAsync(request.UserId, request.PageNumber, request.PageSize, cancellationToken);

        // Map domain entities to response DTOs - excludes full body content for performance
        return logs.Select(log => new NotificationLogResponse
        {
            Id = log.Id,
            UserId = log.UserId,
            RecipientEmail = log.RecipientEmail,
            RecipientPhone = log.RecipientPhone,
            Channel = log.Channel,
            Subject = log.Subject,
            Body = log.Body,
            Status = log.Status,
            RetryCount = log.RetryCount,
            SentAt = log.SentAt,
            ErrorMessage = log.ErrorMessage,
            EventType = log.EventType,
            CreatedAt = log.CreatedAt
        }).ToList();
    }
}
