using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Domain.Events;

public sealed record SubscriptionCreatedEvent(
    Guid EventId,
    DateTime OccurredOn,
    SubscriptionId SubscriptionId,
    GroupId GroupId,
    string ServiceName,
    Money TotalCost) : IDomainEvent;
