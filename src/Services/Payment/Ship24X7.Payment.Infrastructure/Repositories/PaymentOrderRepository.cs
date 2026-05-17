using Microsoft.EntityFrameworkCore;
using Ship24X7.Payment.Application.Interfaces;
using Ship24X7.Payment.Domain.Entities;
using Ship24X7.Payment.Infrastructure.Persistence;

namespace Ship24X7.Payment.Infrastructure.Repositories;

/// <summary>
/// Repository for managing PaymentOrder persistence operations. Handles database CRUD operations and queries.
/// </summary>
public class PaymentOrderRepository : IPaymentOrderRepository
{
    private readonly PaymentDbContext _context;

    public PaymentOrderRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentOrder?> GetByIdAsync(Guid id)
    {
        return await _context.PaymentOrders
            .Include(po => po.Refunds)
            .FirstOrDefaultAsync(po => po.Id == id);
    }

    public async Task<PaymentOrder?> GetByShipmentIdAsync(Guid shipmentId)
    {
        return await _context.PaymentOrders
            .Include(po => po.Refunds)
            .FirstOrDefaultAsync(po => po.ShipmentId == shipmentId);
    }

    public async Task<PaymentOrder?> GetByRazorpayOrderIdAsync(string razorpayOrderId)
    {
        return await _context.PaymentOrders
            .Include(po => po.Refunds)
            .FirstOrDefaultAsync(po => po.RazorpayOrderId == razorpayOrderId);
    }

    public async Task<PaymentOrder?> GetByIdempotencyKeyAsync(string idempotencyKey)
    {
        return await _context.PaymentOrders
            .Include(po => po.Refunds)
            .FirstOrDefaultAsync(po => po.IdempotencyKey == idempotencyKey);
    }

    public async Task<PaymentOrder> AddAsync(PaymentOrder paymentOrder)
    {
        await _context.PaymentOrders.AddAsync(paymentOrder);
        await _context.SaveChangesAsync();
        return paymentOrder;
    }

    public async Task UpdateAsync(PaymentOrder paymentOrder)
    {
        _context.PaymentOrders.Update(paymentOrder);
        await _context.SaveChangesAsync();
    }
}
