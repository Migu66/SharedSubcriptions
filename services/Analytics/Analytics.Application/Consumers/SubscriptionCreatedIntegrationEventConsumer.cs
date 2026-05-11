using Analytics.Application.IntegrationEvents;
using Analytics.Domain.ReadModels;
using Analytics.Domain.Repositories;
using Analytics.Domain.ValueObjects;
using MassTransit;
using SharedSubscriptions.SharedKernel.Domain;

namespace Analytics.Application.Consumers;

/// <summary>
/// Consume SubscriptionCreatedIntegrationEvent y persiste el contexto de la suscripción
/// (nombre del servicio y grupo) para que otros consumidores puedan enriquecer sus
/// proyecciones sin necesidad de consultar Subscriptions Service.
/// </summary>
internal sealed class SubscriptionCreatedIntegrationEventConsumer
    : IConsumer<SubscriptionCreatedIntegrationEvent>
{
    private readonly ISubscriptionContextRepository _subscriptionContextRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubscriptionCreatedIntegrationEventConsumer(
        ISubscriptionContextRepository subscriptionContextRepository,
        IUnitOfWork unitOfWork)
    {
        _subscriptionContextRepository = subscriptionContextRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Consume(ConsumeContext<SubscriptionCreatedIntegrationEvent> context)
    {
        var evt = context.Message;
        var cancellationToken = context.CancellationToken;

        var groupIdResult = GroupId.From(evt.GroupId);
        if (groupIdResult.IsFailure)
            return;

        var existing = await _subscriptionContextRepository
            .GetBySubscriptionIdAsync(evt.SubscriptionId, cancellationToken);

        if (existing is not null)
            return; // ya está registrado, no hace falta volver a guardarlo

        var subscriptionContext = SubscriptionContextReadModel.Create(
            evt.SubscriptionId,
            groupIdResult.Value!,
            evt.ServiceName);

        await _subscriptionContextRepository.AddAsync(subscriptionContext, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
