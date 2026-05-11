using Analytics.Domain.ValueObjects;

namespace Analytics.Domain.ReadModels;

/// <summary>
/// Proyección de solo lectura que acumula el gasto total por nombre de servicio
/// dentro de un grupo.
/// Actualizado al recibir PaymentConfirmedIntegrationEvent.
/// </summary>
public sealed class ServiceSpendingReadModel
{
    public Guid Id { get; init; }
    public GroupId GroupId { get; init; } = default!;
    public string ServiceName { get; init; } = string.Empty;
    public decimal TotalSpent { get; private set; }
    public int PaymentCount { get; private set; }

    // Constructor vacío para EF Core
    private ServiceSpendingReadModel() { }

    public static ServiceSpendingReadModel Create(GroupId groupId, string serviceName)
        => new()
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            ServiceName = serviceName,
            TotalSpent = 0m,
            PaymentCount = 0
        };

    public void RecordPayment(decimal amount)
    {
        TotalSpent += amount;
        PaymentCount++;
    }
}
