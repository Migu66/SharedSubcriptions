using MassTransit;
using MediatR;
using Payments.Application.IntegrationEvents;
using Payments.Domain.Events;

namespace Payments.Infrastructure.EventHandlers;

internal sealed class DebtSettledEventHandler
    : INotificationHandler<DebtSettledEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public DebtSettledEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(
        DebtSettledEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new DebtSettledIntegrationEvent(
            debtId: notification.DebtId,
            debtorId: notification.DebtorId.Value,
            creditorId: notification.CreditorId.Value,
            amount: notification.Amount.Amount,
            currency: notification.Amount.Currency);

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
