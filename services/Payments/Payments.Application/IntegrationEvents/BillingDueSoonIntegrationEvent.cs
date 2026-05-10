using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.ValueObjects;

namespace Payments.Application.IntegrationEvents;

/// <summary>
/// Contrato local del evento publicado por Subscriptions Service.
/// El nombre del exchange se configura en MassTransit para enlazarlo
/// al exchange: Subscriptions.Application.IntegrationEvents.BillingDueSoonIntegrationEvent
/// </summary>
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
