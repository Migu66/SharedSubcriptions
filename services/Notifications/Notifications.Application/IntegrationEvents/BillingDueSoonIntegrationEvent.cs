using Notifications.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Notifications.Application.IntegrationEvents;

/// <summary>
/// Contrato local del evento publicado por Subscriptions Service.
/// Se enlaza al exchange: Subscriptions.Application.IntegrationEvents.BillingDueSoonIntegrationEvent
/// </summary>
public sealed class BillingDueSoonIntegrationEvent : IntegrationEvent
{
    public GroupId GroupId { get; init; }
    public string ServiceName { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; }
    public DateTime BillingDate { get; init; }

    public BillingDueSoonIntegrationEvent(
        GroupId groupId,
        string serviceName,
        decimal totalAmount,
        string currency,
        DateTime billingDate)
    {
        GroupId = groupId;
        ServiceName = serviceName;
        TotalAmount = totalAmount;
        Currency = currency;
        BillingDate = billingDate;
    }
}
