using Microsoft.EntityFrameworkCore;
using Ship24X7.Payment.Application.Interfaces;
using Ship24X7.Payment.Domain.Entities;
using Ship24X7.Payment.Infrastructure.Persistence;

namespace Ship24X7.Payment.Infrastructure.Repositories;

/// <summary>
/// Repository for managing PaymentRefund persistence operations. Handles database CRUD operations and queries.
/// </summary>
public class PaymentRefundRepository : IPaymentRefundRepository
{
    private readonly PaymentDbContext _context;

    public PaymentRefundRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentRefund?> GetByIdAsync(Guid id)
    {
        return await _context.PaymentRefunds
            .Include(pr => pr.PaymentOrder)
            .FirstOrDefaultAsync(pr => pr.Id == id);
    }

    public async Task<PaymentRefund?> GetByRazorpayRefundIdAsync(string razorpayRefundId)
    {
        return await _context.PaymentRefunds
            .Include(pr => pr.PaymentOrder)
            .FirstOrDefaultAsync(pr => pr.RazorpayRefundId == razorpayRefundId);
    }

    public async Task<IEnumerable<PaymentRefund>> GetByPaymentOrderIdAsync(Guid paymentOrderId)
    {
        return await _context.PaymentRefunds
            .Where(pr => pr.PaymentOrderId == paymentOrderId)
            .ToListAsync();
    }

    public async Task<PaymentRefund> AddAsync(PaymentRefund refund)
    {
        await _context.PaymentRefunds.AddAsync(refund);
        await _context.SaveChangesAsync();
        return refund;
    }

    public async Task UpdateAsync(PaymentRefund refund)
    {
        _context.PaymentRefunds.Update(refund);
        await _context.SaveChangesAsync();
    }
}
