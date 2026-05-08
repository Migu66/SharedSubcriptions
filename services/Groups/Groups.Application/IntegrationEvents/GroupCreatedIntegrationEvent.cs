using Groups.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.IntegrationEvents;

public sealed class GroupCreatedIntegrationEvent : IntegrationEvent
{
    public GroupId GroupId { get; init; }
    public UserId AdminId { get; init; }

    public GroupCreatedIntegrationEvent(GroupId groupId, UserId adminId)
    {
        GroupId = groupId;
        AdminId = adminId;
    }
}
