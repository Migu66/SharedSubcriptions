using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor de EF Core que despacha los domain events de los agregados
/// justo antes de persistir los cambios. Esto garantiza que los integration
/// events (publicados vía MassTransit Outbox) queden en la misma transacción
/// que los cambios del agregado.
/// </summary>
internal sealed class DomainEventDispatcherInterceptor : SaveChangesInterceptor
{
    private readonly IPublisher _publisher;

    public DomainEventDispatcherInterceptor(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await DispatchDomainEventsAsync(eventData.Context, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(
        Microsoft.EntityFrameworkCore.DbContext context,
        CancellationToken cancellationToken)
    {
        // Recolectamos todos los agregados con eventos pendientes
        var aggregates = context.ChangeTracker
            .Entries()
            .Select(e => e.Entity)
            .OfType<IHasDomainEvents>()
            .Where(a => a.DomainEvents.Count != 0)
            .ToList();

        // Recogemos los eventos y limpiamos ANTES de publicar para
        // evitar bucles si la publicación desencadena nuevos SaveChanges
        var domainEvents = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        // Publicamos cada evento. Los handlers de infraestructura llaman a
        // IPublishEndpoint.Publish(...) que el Outbox de MassTransit almacena
        // como OutboxMessage en la misma transacción
        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
