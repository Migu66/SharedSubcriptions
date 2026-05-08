using Groups.Application.IntegrationEvents;
using Groups.Domain.Events;
using MassTransit;
using MediatR;

namespace Groups.Infrastructure.EventHandlers;

internal sealed class MemberAddedEventHandler : INotificationHandler<MemberAddedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MemberAddedEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(MemberAddedEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new MemberAddedToGroupIntegrationEvent(
            groupId: notification.GroupId,
            userId: notification.UserId,
            email: notification.Email);

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
