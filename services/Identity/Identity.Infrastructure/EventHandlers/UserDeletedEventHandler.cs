using Identity.Application.IntegrationEvents;
using Identity.Domain.Events;
using MassTransit;
using MediatR;

namespace Identity.Infrastructure.EventHandlers;

internal sealed class UserDeletedEventHandler : INotificationHandler<UserDeletedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public UserDeletedEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(UserDeletedEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new UserDeletedIntegrationEvent(
            userId: notification.UserId,
            email: notification.Email);

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
