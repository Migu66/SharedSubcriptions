using SharedSubscriptions.SharedKernel.Domain;

namespace Analytics.Application.IntegrationEvents;

/// <summary>
/// Contrato local del evento publicado por Groups Service.
/// Se enlaza al exchange: Groups.Application.IntegrationEvents.MemberAddedToGroupIntegrationEvent
/// </summary>
public sealed class MemberAddedToGroupIntegrationEvent : IntegrationEvent
{
    public Guid GroupId { get; init; }
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;

    public MemberAddedToGroupIntegrationEvent() { }
}
