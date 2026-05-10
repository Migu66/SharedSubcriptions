using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.Aggregates;
using Payments.Domain.ValueObjects;

namespace Payments.Domain.Repositories;

public interface IDebtRepository : IRepository<Debt, DebtId>
{
    Task<IReadOnlyList<Debt>> GetPendingByDebtorIdAsync(UserId debtorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Debt>> GetPendingByCreditorIdAsync(UserId creditorId, CancellationToken cancellationToken = default);
}
