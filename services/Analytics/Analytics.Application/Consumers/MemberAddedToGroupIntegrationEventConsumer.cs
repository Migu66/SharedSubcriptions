using Analytics.Application.IntegrationEvents;
using MassTransit;

namespace Analytics.Application.Consumers;

/// <summary>
/// Consume MemberAddedToGroupIntegrationEvent para mantener el contexto de pertenencia
/// a grupos. Actualmente registra el evento para futuras proyecciones que necesiten
/// relacionar usuarios con grupos (p.ej. estadísticas por miembro).
/// </summary>
internal sealed class MemberAddedToGroupIntegrationEventConsumer
    : IConsumer<MemberAddedToGroupIntegrationEvent>
{
    public Task Consume(ConsumeContext<MemberAddedToGroupIntegrationEvent> context)
    {
        // Punto de extensión: cuando se necesiten proyecciones por miembro,
        // se puede registrar aquí la pertenencia usuario-grupo.
        return Task.CompletedTask;
    }
}
