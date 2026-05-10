using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.ValueObjects;

namespace Payments.Application.IntegrationEvents;

public sealed class DebtSettledIntegrationEvent : IntegrationEvent
{
    public DebtId DebtId { get; init; }
    public Guid DebtorId { get; init; }
    public Guid CreditorId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    public DebtSettledIntegrationEvent(
        DebtId debtId,
        Guid debtorId,
        Guid creditorId,
        decimal amount,
        string currency)
    {
        DebtId = debtId;
        DebtorId = debtorId;
        CreditorId = creditorId;
        Amount = amount;
        Currency = currency;
    }
}
