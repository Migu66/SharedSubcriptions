using SharedSubscriptions.SharedKernel.Domain;

namespace Analytics.Application.IntegrationEvents;

/// <summary>
/// Contrato local del evento publicado por Payments Service.
/// Se enlaza al exchange: Payments.Application.IntegrationEvents.PaymentConfirmedIntegrationEvent
/// </summary>
public sealed class PaymentConfirmedIntegrationEvent : IntegrationEvent
{
    public Guid PaymentRecordId { get; init; }
    public Guid SubscriptionId { get; init; }
    public Guid GroupId { get; init; }
    public Guid AdminId { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public IReadOnlyList<MemberQuotaAnalyticsDto> Quotas { get; init; } = [];

    public PaymentConfirmedIntegrationEvent() { }
}

public sealed record MemberQuotaAnalyticsDto(
    Guid MemberId,
    decimal Amount,
    string Currency,
    bool IsProrrated);
