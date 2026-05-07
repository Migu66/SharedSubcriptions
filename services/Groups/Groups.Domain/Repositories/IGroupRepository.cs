using Groups.Domain.Aggregates;
using Groups.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Domain.Repositories;

public interface IGroupRepository : IRepository<Group, GroupId>
{
    Task<IReadOnlyList<Group>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
}
