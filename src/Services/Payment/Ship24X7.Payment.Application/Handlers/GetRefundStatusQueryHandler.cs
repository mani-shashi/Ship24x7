using MediatR;
using Ship24X7.Payment.Application.DTOs;
using Ship24X7.Payment.Application.Interfaces;
using Ship24X7.Payment.Application.Queries;

namespace Ship24X7.Payment.Application.Handlers;

/// <summary>
/// Handler for processing GetRefundStatus query requests.
/// Retrieves refund details by refund ID from the database.
/// Implements read-only query logic for CQRS pattern.
/// Returns RefundDto with refund details or null if not found.
/// Used by RefundController to fetch refund status and information.
/// </summary>
public class GetRefundStatusQueryHandler : IRequestHandler<GetRefundStatusQuery, RefundDto?>
{
    private readonly IPaymentRefundRepository _refundRepository;

    /// <summary>
    /// Initializes a new instance of the GetRefundStatusQueryHandler class.
    /// Sets up dependency for repository access.
    /// </summary>
    /// <param name="refundRepository">Repository for refund data access</param>
    public GetRefundStatusQueryHandler(IPaymentRefundRepository refundRepository)
    {
        _refundRepository = refundRepository;
    }

    /// <summary>
    /// Handles the GetRefundStatus query with simple retrieval logic.
    /// Step-by-step logic flow:
    /// 1. Retrieves refund from database using refund ID
    /// 2. If refund not found, returns null
    /// 3. Maps refund entity to RefundDto
    /// 4. Includes all refund details: IDs, payment order ID, Razorpay refund ID, amount, currency, status, timestamps, reason, initiator
    /// 5. Converts RefundStatus enum to string for API response
    /// 6. Returns RefundDto for controller response
    /// Used by admin dashboards to track refund status (Initiated, Processed, Failed).
    /// Only accessible by Admin_User and System_Admin roles.
    /// </summary>
    /// <param name="request">Query containing refund ID to retrieve</param>
    /// <param name="cancellationToken">Cancellation token for async operation</param>
    /// <returns>RefundDto with refund details if found; null if not found</returns>
    public async Task<RefundDto?> Handle(GetRefundStatusQuery request, CancellationToken cancellationToken)
    {
        var refund = await _refundRepository.GetByIdAsync(request.RefundId);
        if (refund == null)
        {
            return null;
        }

        return new RefundDto
        {
            Id = refund.Id,
            PaymentOrderId = refund.PaymentOrderId,
            RazorpayRefundId = refund.RazorpayRefundId,
            Amount = refund.Amount,
            Currency = refund.Currency,
            Status = refund.Status.ToString(),
            ProcessedAt = refund.ProcessedAt,
            Reason = refund.Reason,
            InitiatedBy = refund.InitiatedBy,
            CreatedAt = refund.CreatedAt
        };
    }
}
