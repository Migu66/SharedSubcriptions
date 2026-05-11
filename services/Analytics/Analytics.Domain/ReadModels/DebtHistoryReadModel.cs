using Analytics.Domain.ValueObjects;

namespace Analytics.Domain.ReadModels;

/// <summary>
/// Proyección de solo lectura que acumula el historial de deudas de un usuario:
/// cuánto debe en total, cuánto ya ha saldado y cuántas deudas siguen pendientes.
/// Actualizado al recibir DebtCreatedIntegrationEvent y DebtSettledIntegrationEvent.
/// </summary>
public sealed class DebtHistoryReadModel
{
    public Guid Id { get; init; }
    public UserId UserId { get; init; } = default!;
    public decimal TotalDebt { get; private set; }
    public decimal TotalSettled { get; private set; }
    public int PendingCount { get; private set; }

    // Constructor vacío para EF Core
    private DebtHistoryReadModel() { }

    public static DebtHistoryReadModel Create(UserId userId)
        => new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TotalDebt = 0m,
            TotalSettled = 0m,
            PendingCount = 0
        };

    public void AddDebt(decimal amount)
    {
        TotalDebt += amount;
        PendingCount++;
    }

    public void SettleDebt(decimal amount)
    {
        TotalSettled += amount;
        PendingCount = Math.Max(0, PendingCount - 1);
    }
}
