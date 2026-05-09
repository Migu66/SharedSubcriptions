using Identity.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Identity.Application.IntegrationEvents;

public sealed class UserDeletedIntegrationEvent : IntegrationEvent
{
    public UserId UserId { get; init; }
    public string Email { get; init; }

    public UserDeletedIntegrationEvent(UserId userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}
