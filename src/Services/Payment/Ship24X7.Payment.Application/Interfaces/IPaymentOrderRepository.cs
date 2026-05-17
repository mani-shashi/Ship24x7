using Ship24X7.Payment.Domain.Entities;

namespace Ship24X7.Payment.Application.Interfaces;

/// <summary>
/// Repository for managing IPaymentOrder persistence operations. Handles database CRUD operations and queries.
/// </summary>
public interface IPaymentOrderRepository
{
    Task<PaymentOrder?> GetByIdAsync(Guid id);
    Task<PaymentOrder?> GetByShipmentIdAsync(Guid shipmentId);
    Task<PaymentOrder?> GetByRazorpayOrderIdAsync(string razorpayOrderId);
    Task<PaymentOrder?> GetByIdempotencyKeyAsync(string idempotencyKey);
    Task<PaymentOrder> AddAsync(PaymentOrder paymentOrder);
    Task UpdateAsync(PaymentOrder paymentOrder);
}
