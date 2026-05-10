using MassTransit;
using Payments.Application.IntegrationEvents;

namespace Payments.Infrastructure.Messaging;

/// <summary>
/// Consume el evento BillingDueSoonIntegrationEvent publicado por Subscriptions Service.
/// Reacciona cuando se acerca la fecha de cobro de una suscripción para preparar
/// el contexto necesario (p.ej. caché de miembros activos) antes del ciclo de pago.
/// El pago real se registra cuando el admin confirma con ConfirmAdminPaymentCommand.
/// </summary>
internal sealed class BillingDueSoonIntegrationEventConsumer
    : IConsumer<BillingDueSoonIntegrationEvent>
{
    public Task Consume(ConsumeContext<BillingDueSoonIntegrationEvent> context)
    {
        // Punto de extensión: aquí se puede preparar la lógica previa al ciclo de pago,
        // como marcar suscripciones como "en periodo de cobro" o enviar señales internas.
        // En el MVP, la acción real es la confirmación manual del admin.
        return Task.CompletedTask;
    }
}
