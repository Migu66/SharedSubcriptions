using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Application.IntegrationEvents;

/// <summary>
/// Contrato local del evento publicado por Groups Service.
/// El nombre del exchange se configura en MassTransit para enlazarlo
/// al exchange: Groups.Application.IntegrationEvents.MemberRemovedFromGroupIntegrationEvent
/// </summary>
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
