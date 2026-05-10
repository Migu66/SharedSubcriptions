using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Application.IntegrationEvents;

public sealed class SubscriptionPriceChangedIntegrationEvent : IntegrationEvent
{
    public SubscriptionId SubscriptionId { get; init; }
    public GroupId GroupId { get; init; }
    public decimal OldAmount { get; init; }
    public string OldCurrency { get; init; }
    public decimal NewAmount { get; init; }
    public string NewCurrency { get; init; }

    public SubscriptionPriceChangedIntegrationEvent(
        SubscriptionId subscriptionId,
        GroupId groupId,
        decimal oldAmount,
        string oldCurrency,
        decimal newAmount,
        string newCurrency)
    {
        SubscriptionId = subscriptionId;
        GroupId = groupId;
        OldAmount = oldAmount;
        OldCurrency = oldCurrency;
        NewAmount = newAmount;
        NewCurrency = newCurrency;
    }
}
