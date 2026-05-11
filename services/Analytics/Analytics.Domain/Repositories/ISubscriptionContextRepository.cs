using Analytics.Domain.ReadModels;

namespace Analytics.Domain.Repositories;

public interface ISubscriptionContextRepository
{
    Task<SubscriptionContextReadModel?> GetBySubscriptionIdAsync(Guid subscriptionId, CancellationToken cancellationToken = default);
    Task AddAsync(SubscriptionContextReadModel readModel, CancellationToken cancellationToken = default);
}
