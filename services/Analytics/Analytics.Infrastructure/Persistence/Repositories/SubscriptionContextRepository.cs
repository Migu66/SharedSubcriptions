using Analytics.Domain.ReadModels;
using Analytics.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Infrastructure.Persistence.Repositories;

internal sealed class SubscriptionContextRepository : ISubscriptionContextRepository
{
    private readonly AnalyticsDbContext _context;

    public SubscriptionContextRepository(AnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionContextReadModel?> GetBySubscriptionIdAsync(
        Guid subscriptionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.SubscriptionContexts
            .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId, cancellationToken);
    }

    public async Task AddAsync(
        SubscriptionContextReadModel readModel,
        CancellationToken cancellationToken = default)
    {
        await _context.SubscriptionContexts.AddAsync(readModel, cancellationToken);
    }
}
