using Microsoft.EntityFrameworkCore;
using Payments.Domain.Aggregates;
using Payments.Domain.Enums;
using Payments.Domain.Repositories;
using Payments.Domain.ValueObjects;

namespace Payments.Infrastructure.Persistence.Repositories;

internal sealed class DebtRepository : IDebtRepository
{
    private readonly PaymentsDbContext _context;

    public DebtRepository(PaymentsDbContext context)
    {
        _context = context;
    }

    public async Task<Debt?> GetByIdAsync(
        DebtId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Debts
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task AddAsync(
        Debt aggregate,
        CancellationToken cancellationToken = default)
    {
        await _context.Debts.AddAsync(aggregate, cancellationToken);
    }

    public void Update(Debt aggregate)
    {
        _context.Debts.Update(aggregate);
    }

    public void Remove(Debt aggregate)
    {
        _context.Debts.Remove(aggregate);
    }

    public async Task<IReadOnlyList<Debt>> GetPendingByDebtorIdAsync(
        UserId debtorId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Debts
            .Where(d => d.DebtorId == debtorId && d.Status == DebtStatus.Pending)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Debt>> GetPendingByCreditorIdAsync(
        UserId creditorId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Debts
            .Where(d => d.CreditorId == creditorId && d.Status == DebtStatus.Pending)
            .ToListAsync(cancellationToken);
    }
}
