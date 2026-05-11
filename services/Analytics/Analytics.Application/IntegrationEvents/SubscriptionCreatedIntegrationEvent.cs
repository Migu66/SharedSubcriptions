using SharedSubscriptions.SharedKernel.Domain;

namespace Analytics.Application.IntegrationEvents;

/// <summary>
/// Contrato local del evento publicado por Subscriptions Service.
/// Se enlaza al exchange: Subscriptions.Application.IntegrationEvents.SubscriptionCreatedIntegrationEvent
/// </summary>
public sealed class SubscriptionCreatedIntegrationEvent : IntegrationEvent
{
    public Guid SubscriptionId { get; init; }
    public Guid GroupId { get; init; }
    public string ServiceName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = string.Empty;

    public SubscriptionCreatedIntegrationEvent() { }
}
