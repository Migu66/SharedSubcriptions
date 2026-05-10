using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.Aggregates;
using Payments.Domain.ValueObjects;

namespace Payments.Domain.Repositories;

public interface IPaymentRecordRepository : IRepository<PaymentRecord, PaymentRecordId>
{
    Task<IReadOnlyList<PaymentRecord>> GetBySubscriptionIdAsync(SubscriptionId subscriptionId, CancellationToken cancellationToken = default);
}
