using Groups.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.IntegrationEvents;

public sealed class MemberRemovedFromGroupIntegrationEvent : IntegrationEvent
{
    public GroupId GroupId { get; init; }
    public UserId UserId { get; init; }

    public MemberRemovedFromGroupIntegrationEvent(GroupId groupId, UserId userId)
    {
        GroupId = groupId;
        UserId = userId;
    }
}
