using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.Errors;
using Payments.Domain.Events;
using Payments.Domain.ValueObjects;

namespace Payments.Domain.Aggregates;

public sealed class PaymentRecord : AggregateRoot<PaymentRecordId>
{
    public SubscriptionId SubscriptionId { get; private init; } = null!;
    public GroupId GroupId { get; private init; } = null!;
    public UserId AdminId { get; private init; } = null!;
    public Money TotalAmount { get; private init; } = null!;
    public DateTime PaidAt { get; private init; }

    private readonly List<MemberQuota> _memberQuotas = [];
    public IReadOnlyCollection<MemberQuota> MemberQuotas => _memberQuotas.AsReadOnly();

    private PaymentRecord() { }

    public static Result<PaymentRecord> Create(
        SubscriptionId subscriptionId,
        GroupId groupId,
        UserId adminId,
        Money totalAmount,
        IReadOnlyList<MemberQuota> memberQuotas,
        DateTime paidAt)
    {
        if (memberQuotas is null || memberQuotas.Count == 0)
            return Result.Failure<PaymentRecord>(PaymentRecordErrors.EmptyQuotas);

        var paymentRecord = new PaymentRecord
        {
            Id = PaymentRecordId.New(),
            SubscriptionId = subscriptionId,
            GroupId = groupId,
            AdminId = adminId,
            TotalAmount = totalAmount,
            PaidAt = paidAt
        };

        paymentRecord._memberQuotas.AddRange(memberQuotas);

        paymentRecord.RaiseDomainEvent(new PaymentRecordCreatedEvent(
            EventId: Guid.NewGuid(),
            OccurredOn: paidAt,
            PaymentRecordId: paymentRecord.Id,
            SubscriptionId: subscriptionId,
            GroupId: groupId,
            AdminId: adminId,
            Quotas: memberQuotas));

        return Result.Success(paymentRecord);
    }
}
