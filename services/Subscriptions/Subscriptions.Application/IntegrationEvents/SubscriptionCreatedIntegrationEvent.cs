using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Application.IntegrationEvents;

public sealed class SubscriptionCreatedIntegrationEvent : IntegrationEvent
{
    public SubscriptionId SubscriptionId { get; init; }
    public GroupId GroupId { get; init; }
    public string ServiceName { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; }

    public SubscriptionCreatedIntegrationEvent(
        SubscriptionId subscriptionId,
        GroupId groupId,
        string serviceName,
        decimal totalAmount,
        string currency)
    {
        SubscriptionId = subscriptionId;
        GroupId = groupId;
        ServiceName = serviceName;
        TotalAmount = totalAmount;
        Currency = currency;
    }
}
