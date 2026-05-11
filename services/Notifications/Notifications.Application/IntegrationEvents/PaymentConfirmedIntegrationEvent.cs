using Notifications.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Notifications.Application.IntegrationEvents;

/// <summary>
/// Contrato local del evento publicado por Payments Service.
/// Se enlaza al exchange: Payments.Application.IntegrationEvents.PaymentConfirmedIntegrationEvent
/// </summary>
public sealed class PaymentConfirmedIntegrationEvent : IntegrationEvent
{
    public GroupId GroupId { get; init; }
    public string ServiceName { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; }
    public IReadOnlyList<MemberQuotaDto> Quotas { get; init; }

    public PaymentConfirmedIntegrationEvent(
        GroupId groupId,
        string serviceName,
        decimal totalAmount,
        string currency,
        IReadOnlyList<MemberQuotaDto> quotas)
    {
        GroupId = groupId;
        ServiceName = serviceName;
        TotalAmount = totalAmount;
        Currency = currency;
        Quotas = quotas;
    }
}

public sealed record MemberQuotaDto(
    Guid MemberId,
    decimal Amount,
    string Currency);
