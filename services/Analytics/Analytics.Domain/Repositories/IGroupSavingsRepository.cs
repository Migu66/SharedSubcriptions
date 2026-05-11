using Analytics.Domain.ReadModels;
using Analytics.Domain.ValueObjects;

namespace Analytics.Domain.Repositories;

public interface IGroupSavingsRepository
{
    Task<GroupSavingsReadModel?> GetByGroupIdAndYearAsync(GroupId groupId, int year, CancellationToken cancellationToken = default);
    Task AddAsync(GroupSavingsReadModel readModel, CancellationToken cancellationToken = default);
    Task UpdateAsync(GroupSavingsReadModel readModel, CancellationToken cancellationToken = default);
}
