using Analytics.Domain.ReadModels;
using Analytics.Domain.Repositories;
using Analytics.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Infrastructure.Persistence.Repositories;

internal sealed class DebtHistoryRepository : IDebtHistoryRepository
{
    private readonly AnalyticsDbContext _context;

    public DebtHistoryRepository(AnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<DebtHistoryReadModel?> GetByUserIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.DebtHistories
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(
        DebtHistoryReadModel readModel,
        CancellationToken cancellationToken = default)
    {
        await _context.DebtHistories.AddAsync(readModel, cancellationToken);
    }

    public Task UpdateAsync(
        DebtHistoryReadModel readModel,
        CancellationToken cancellationToken = default)
    {
        _context.DebtHistories.Update(readModel);
        return Task.CompletedTask;
    }
}
