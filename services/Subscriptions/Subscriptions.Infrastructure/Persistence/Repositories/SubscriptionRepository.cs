using Microsoft.EntityFrameworkCore;
using Subscriptions.Domain.Aggregates;
using Subscriptions.Domain.Repositories;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Infrastructure.Persistence.Repositories;

internal sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly SubscriptionsDbContext _context;

    public SubscriptionRepository(SubscriptionsDbContext context)
    {
        _context = context;
    }

    public async Task<Subscription?> GetByIdAsync(
        SubscriptionId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task AddAsync(
        Subscription aggregate,
        CancellationToken cancellationToken = default)
    {
        await _context.Subscriptions.AddAsync(aggregate, cancellationToken);
    }

    public void Update(Subscription aggregate)
    {
        _context.Subscriptions.Update(aggregate);
    }

    public void Remove(Subscription aggregate)
    {
        _context.Subscriptions.Remove(aggregate);
    }

    public async Task<IReadOnlyList<Subscription>> GetByGroupIdAsync(
        GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(s => s.GroupId == groupId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetDueSoonAsync(
        DateTime threshold,
        CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(s => s.IsActive && s.BillingSchedule.NextBillingDate <= threshold)
            .ToListAsync(cancellationToken);
    }
}
