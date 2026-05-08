using Groups.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.IntegrationEvents;

public sealed class MemberAddedToGroupIntegrationEvent : IntegrationEvent
{
    public GroupId GroupId { get; init; }
    public UserId UserId { get; init; }
    public string Email { get; init; }

    public MemberAddedToGroupIntegrationEvent(GroupId groupId, UserId userId, string email)
    {
        GroupId = groupId;
        UserId = userId;
        Email = email;
    }
}
