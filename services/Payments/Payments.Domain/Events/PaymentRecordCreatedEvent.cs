using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.ValueObjects;

namespace Payments.Domain.Events;

public sealed record PaymentRecordCreatedEvent(
    Guid EventId,
    DateTime OccurredOn,
    PaymentRecordId PaymentRecordId,
    SubscriptionId SubscriptionId,
    GroupId GroupId,
    UserId AdminId,
    IReadOnlyList<MemberQuota> Quotas) : IDomainEvent;
