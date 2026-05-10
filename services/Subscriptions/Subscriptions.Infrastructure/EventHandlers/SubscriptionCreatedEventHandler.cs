using MassTransit;
using MediatR;
using Subscriptions.Application.IntegrationEvents;
using Subscriptions.Domain.Events;

namespace Subscriptions.Infrastructure.EventHandlers;

internal sealed class SubscriptionCreatedEventHandler
    : INotificationHandler<SubscriptionCreatedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public SubscriptionCreatedEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(SubscriptionCreatedEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new SubscriptionCreatedIntegrationEvent(
            subscriptionId: notification.SubscriptionId,
            groupId: notification.GroupId,
            serviceName: notification.ServiceName,
            totalAmount: notification.TotalCost.Amount,
            currency: notification.TotalCost.Currency);

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
