using Groups.Application.IntegrationEvents;
using Groups.Domain.Events;
using MassTransit;
using MediatR;

namespace Groups.Infrastructure.EventHandlers;

internal sealed class MemberRemovedEventHandler : INotificationHandler<MemberRemovedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MemberRemovedEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(MemberRemovedEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new MemberRemovedFromGroupIntegrationEvent(
            groupId: notification.GroupId,
            userId: notification.UserId);

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
