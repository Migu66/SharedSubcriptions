using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.ValueObjects;

namespace Payments.Application.IntegrationEvents;

public sealed class DebtCreatedIntegrationEvent : IntegrationEvent
{
    public DebtId DebtId { get; init; }
    public SubscriptionId SubscriptionId { get; init; }
    public Guid DebtorId { get; init; }
    public Guid CreditorId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    public DebtCreatedIntegrationEvent(
        DebtId debtId,
        SubscriptionId subscriptionId,
        Guid debtorId,
        Guid creditorId,
        decimal amount,
        string currency)
    {
        DebtId = debtId;
        SubscriptionId = subscriptionId;
        DebtorId = debtorId;
        CreditorId = creditorId;
        Amount = amount;
        Currency = currency;
    }
}
