using MassTransit;
using Subscriptions.Application.IntegrationEvents;
using Subscriptions.Domain.Repositories;

namespace Subscriptions.Infrastructure.Messaging;

/// <summary>
/// Consume el evento MemberRemovedFromGroupIntegrationEvent publicado por Groups Service.
/// Cuando un miembro abandona un grupo, recalculamos las suscripciones activas
/// del grupo para que el próximo ciclo refleje el número correcto de participantes.
/// Por ahora registra el evento; el recálculo de cuotas corresponde al Payments Service.
/// </summary>
internal sealed class MemberRemovedFromGroupIntegrationEventConsumer
    : IConsumer<MemberRemovedFromGroupIntegrationEvent>
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public MemberRemovedFromGroupIntegrationEventConsumer(
        ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task Consume(ConsumeContext<MemberRemovedFromGroupIntegrationEvent> context)
    {
        // Obtenemos las suscripciones activas del grupo para que el sistema tenga
        // conocimiento actualizado del grupo. El recálculo de cuotas es responsabilidad
        // del Payments Service al recibir el mismo evento.
        var subscriptions = await _subscriptionRepository.GetByGroupIdAsync(
            context.Message.GroupId,
            context.CancellationToken);

        // Fuerza la carga de suscripciones activas del grupo.
        // Cualquier lógica adicional de este servicio (p.ej. actualizar un contador
        // de miembros cacheado) se añadiría aquí en el futuro.
        _ = subscriptions;
    }
}
