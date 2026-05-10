using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.ValueObjects;

namespace Payments.Application.IntegrationEvents;

/// <summary>
/// Contrato local del evento publicado por Groups Service.
/// El nombre del exchange se configura en MassTransit para enlazarlo
/// al exchange: Groups.Application.IntegrationEvents.MemberAddedToGroupIntegrationEvent
/// </summary>
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
