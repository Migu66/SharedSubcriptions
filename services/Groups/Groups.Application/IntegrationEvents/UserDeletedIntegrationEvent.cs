namespace Groups.Application.IntegrationEvents;

/// <summary>
/// Contrato local que mapea el mensaje publicado por Identity Service.
/// La forma del JSON publicado por Identity es:
///   { "UserId": { "Value": "guid" }, "Email": "...", "EventId": "...", "OccurredOn": "..." }
/// porque Identity.Domain.ValueObjects.UserId es un record con propiedad Value.
/// </summary>
public sealed class UserDeletedIntegrationEvent
{
    public UserIdValue UserId { get; init; } = default!;
    public string Email { get; init; } = string.Empty;

    public sealed class UserIdValue
    {
        public Guid Value { get; init; }
    }
}
