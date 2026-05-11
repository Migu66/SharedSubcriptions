using Analytics.Domain.ValueObjects;

namespace Analytics.Domain.ReadModels;

/// <summary>
/// Almacena el contexto de una suscripción (nombre del servicio y grupo al que pertenece)
/// para que los consumidores puedan enriquecer las proyecciones de gasto sin depender
/// de otros servicios en tiempo de consulta.
/// Actualizado al recibir SubscriptionCreatedIntegrationEvent.
/// </summary>
public sealed class SubscriptionContextReadModel
{
    public Guid SubscriptionId { get; init; }
    public GroupId GroupId { get; init; } = default!;
    public string ServiceName { get; init; } = string.Empty;

    // Constructor vacío para EF Core
    private SubscriptionContextReadModel() { }

    public static SubscriptionContextReadModel Create(Guid subscriptionId, GroupId groupId, string serviceName)
        => new()
        {
            SubscriptionId = subscriptionId,
            GroupId = groupId,
            ServiceName = serviceName
        };
}
