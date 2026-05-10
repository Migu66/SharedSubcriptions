using Microsoft.EntityFrameworkCore;
using Payments.Domain.Aggregates;
using Payments.Domain.Repositories;
using Payments.Domain.ValueObjects;

namespace Payments.Infrastructure.Persistence.Repositories;

internal sealed class PaymentRecordRepository : IPaymentRecordRepository
{
    private readonly PaymentsDbContext _context;

    public PaymentRecordRepository(PaymentsDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentRecord?> GetByIdAsync(
        PaymentRecordId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.PaymentRecords
            .Include(p => p.MemberQuotas)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(
        PaymentRecord aggregate,
        CancellationToken cancellationToken = default)
    {
        await _context.PaymentRecords.AddAsync(aggregate, cancellationToken);
    }

    public void Update(PaymentRecord aggregate)
    {
        _context.PaymentRecords.Update(aggregate);
    }

    public void Remove(PaymentRecord aggregate)
    {
        _context.PaymentRecords.Remove(aggregate);
    }

    public async Task<IReadOnlyList<PaymentRecord>> GetBySubscriptionIdAsync(
        SubscriptionId subscriptionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PaymentRecords
            .Include(p => p.MemberQuotas)
            .Where(p => p.SubscriptionId == subscriptionId)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync(cancellationToken);
    }
}
