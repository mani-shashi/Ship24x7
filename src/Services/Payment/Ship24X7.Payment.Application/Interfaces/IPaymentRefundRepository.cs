using Ship24X7.Payment.Domain.Entities;

namespace Ship24X7.Payment.Application.Interfaces;

/// <summary>
/// Repository for managing IPaymentRefund persistence operations. Handles database CRUD operations and queries.
/// </summary>
public interface IPaymentRefundRepository
{
    Task<PaymentRefund?> GetByIdAsync(Guid id);
    Task<PaymentRefund?> GetByRazorpayRefundIdAsync(string razorpayRefundId);
    Task<IEnumerable<PaymentRefund>> GetByPaymentOrderIdAsync(Guid paymentOrderId);
    Task<PaymentRefund> AddAsync(PaymentRefund refund);
    Task UpdateAsync(PaymentRefund refund);
}
