using Identity.Application.IntegrationEvents;
using Identity.Domain.Events;
using Identity.Domain.ValueObjects;
using MassTransit;
using MediatR;

namespace Identity.Infrastructure.EventHandlers;

internal sealed class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public UserRegisteredEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new UserRegisteredIntegrationEvent(
            userId: notification.UserId,
            email: notification.Email,
            firstName: notification.FirstName,
            lastName: notification.LastName);

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
