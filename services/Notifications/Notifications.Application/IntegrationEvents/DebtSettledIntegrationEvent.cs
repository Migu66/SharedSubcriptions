using Notifications.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Notifications.Application.IntegrationEvents;

/// <summary>
/// Contrato local del evento publicado por Payments Service.
/// Se enlaza al exchange: Payments.Application.IntegrationEvents.DebtSettledIntegrationEvent
/// </summary>
public sealed class DebtSettledIntegrationEvent : IntegrationEvent
{
    public Guid DebtorId { get; init; }
    public Guid CreditorId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    public DebtSettledIntegrationEvent(
        Guid debtorId,
        Guid creditorId,
        decimal amount,
        string currency)
    {
        DebtorId = debtorId;
        CreditorId = creditorId;
        Amount = amount;
        Currency = currency;
    }
}
