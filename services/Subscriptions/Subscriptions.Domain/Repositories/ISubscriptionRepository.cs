using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.Aggregates;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Domain.Repositories;

public interface ISubscriptionRepository : IRepository<Subscription, SubscriptionId>
{
    Task<IReadOnlyList<Subscription>> GetByGroupIdAsync(GroupId groupId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Subscription>> GetDueSoonAsync(DateTime threshold, CancellationToken cancellationToken = default);
}
