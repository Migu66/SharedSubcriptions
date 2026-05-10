using MassTransit;
using MediatR;
using Subscriptions.Application.IntegrationEvents;
using Subscriptions.Domain.Events;

namespace Subscriptions.Infrastructure.EventHandlers;

internal sealed class SubscriptionPriceChangedEventHandler
    : INotificationHandler<SubscriptionPriceChangedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public SubscriptionPriceChangedEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(
        SubscriptionPriceChangedEvent notification,
        CancellationToken cancellationToken)
    {
        var integrationEvent = new SubscriptionPriceChangedIntegrationEvent(
            subscriptionId: notification.SubscriptionId,
            groupId: notification.GroupId,
            oldAmount: notification.OldCost.Amount,
            oldCurrency: notification.OldCost.Currency,
            newAmount: notification.NewCost.Amount,
            newCurrency: notification.NewCost.Currency);

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
