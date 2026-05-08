using Groups.Application.IntegrationEvents;
using Groups.Domain.Events;
using MassTransit;
using MediatR;

namespace Groups.Infrastructure.EventHandlers;

internal sealed class GroupCreatedEventHandler : INotificationHandler<GroupCreatedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public GroupCreatedEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(GroupCreatedEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new GroupCreatedIntegrationEvent(
            groupId: notification.GroupId,
            adminId: notification.AdminId);

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}
