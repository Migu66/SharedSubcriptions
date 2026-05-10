using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Domain.Events;

public sealed record BillingCycleAdvancedEvent(
    Guid EventId,
    DateTime OccurredOn,
    SubscriptionId SubscriptionId,
    DateTime NewBillingDate) : IDomainEvent;
