using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Application.IntegrationEvents;

public sealed class BillingDueSoonIntegrationEvent : IntegrationEvent
{
    public SubscriptionId SubscriptionId { get; init; }
    public GroupId GroupId { get; init; }
    public string ServiceName { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; }
    public DateTime BillingDate { get; init; }

    public BillingDueSoonIntegrationEvent(
        SubscriptionId subscriptionId,
        GroupId groupId,
        string serviceName,
        decimal totalAmount,
        string currency,
        DateTime billingDate)
    {
        SubscriptionId = subscriptionId;
        GroupId = groupId;
        ServiceName = serviceName;
        TotalAmount = totalAmount;
        Currency = currency;
        BillingDate = billingDate;
    }
}
