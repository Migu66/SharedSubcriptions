using Analytics.Domain.ReadModels;
using Analytics.Domain.ValueObjects;

namespace Analytics.Domain.Repositories;

public interface IDebtHistoryRepository
{
    Task<DebtHistoryReadModel?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default);
    Task AddAsync(DebtHistoryReadModel readModel, CancellationToken cancellationToken = default);
    Task UpdateAsync(DebtHistoryReadModel readModel, CancellationToken cancellationToken = default);
}
