using MassTransit;
using Payments.Application.IntegrationEvents;

namespace Payments.Infrastructure.Messaging;

/// <summary>
/// Consume el evento MemberAddedToGroupIntegrationEvent publicado por Groups Service.
/// Cuando se añade un nuevo miembro a un grupo, el Payments Service toma nota
/// para que el siguiente ConfirmAdminPaymentCommand pueda calcular el prorrateo correcto.
/// El cálculo real del prorrateo ocurre en ConfirmAdminPaymentCommandHandler,
/// que recibe la lista actualizada de miembros desde la petición del cliente.
/// </summary>
internal sealed class MemberAddedToGroupIntegrationEventConsumer
    : IConsumer<MemberAddedToGroupIntegrationEvent>
{
    public Task Consume(ConsumeContext<MemberAddedToGroupIntegrationEvent> context)
    {
        // Punto de extensión: aquí se puede registrar al nuevo miembro en una proyección
        // local del Payments Service para conocer el número actual de participantes
        // sin necesidad de consultar al Groups Service en tiempo de pago.
        return Task.CompletedTask;
    }
}
