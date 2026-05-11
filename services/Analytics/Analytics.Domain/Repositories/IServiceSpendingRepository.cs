using Analytics.Domain.ReadModels;
using Analytics.Domain.ValueObjects;

namespace Analytics.Domain.Repositories;

public interface IServiceSpendingRepository
{
    Task<ServiceSpendingReadModel?> GetByGroupIdAndServiceNameAsync(GroupId groupId, string serviceName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceSpendingReadModel>> GetByGroupIdAsync(GroupId groupId, CancellationToken cancellationToken = default);
    Task AddAsync(ServiceSpendingReadModel readModel, CancellationToken cancellationToken = default);
    Task UpdateAsync(ServiceSpendingReadModel readModel, CancellationToken cancellationToken = default);
}
