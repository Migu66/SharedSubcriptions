using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Domain.Events;

public sealed record SubscriptionDeactivatedEvent(
    Guid EventId,
    DateTime OccurredOn,
    SubscriptionId SubscriptionId) : IDomainEvent;
