using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Domain.Events;

public sealed record SubscriptionPriceChangedEvent(
    Guid EventId,
    DateTime OccurredOn,
    SubscriptionId SubscriptionId,
    GroupId GroupId,
    Money OldCost,
    Money NewCost) : IDomainEvent;
