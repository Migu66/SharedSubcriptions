using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.ValueObjects;

namespace Payments.Application.IntegrationEvents;

public sealed class PaymentConfirmedIntegrationEvent : IntegrationEvent
{
    public PaymentRecordId PaymentRecordId { get; init; }
    public SubscriptionId SubscriptionId { get; init; }
    public GroupId GroupId { get; init; }
    public Guid AdminId { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; }
    public IReadOnlyList<MemberQuotaIntegrationDto> Quotas { get; init; }

    public PaymentConfirmedIntegrationEvent(
        PaymentRecordId paymentRecordId,
        SubscriptionId subscriptionId,
        GroupId groupId,
        Guid adminId,
        decimal totalAmount,
        string currency,
        IReadOnlyList<MemberQuotaIntegrationDto> quotas)
    {
        PaymentRecordId = paymentRecordId;
        SubscriptionId = subscriptionId;
        GroupId = groupId;
        AdminId = adminId;
        TotalAmount = totalAmount;
        Currency = currency;
        Quotas = quotas;
    }
}

public sealed record MemberQuotaIntegrationDto(
    Guid MemberId,
    decimal Amount,
    string Currency,
    bool IsProrrated);
