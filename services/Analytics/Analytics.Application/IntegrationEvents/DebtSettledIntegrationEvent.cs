using SharedSubscriptions.SharedKernel.Domain;

namespace Analytics.Application.IntegrationEvents;

/// <summary>
/// Contrato local del evento publicado por Payments Service.
/// Se enlaza al exchange: Payments.Application.IntegrationEvents.DebtSettledIntegrationEvent
/// </summary>
public sealed class DebtSettledIntegrationEvent : IntegrationEvent
{
    public Guid DebtId { get; init; }
    public Guid DebtorId { get; init; }
    public Guid CreditorId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;

    public DebtSettledIntegrationEvent() { }
}
