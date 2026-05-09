using Identity.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Identity.Application.IntegrationEvents;

public sealed class UserRegisteredIntegrationEvent : IntegrationEvent
{
    public UserId UserId { get; init; }
    public string Email { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }

    public UserRegisteredIntegrationEvent(UserId userId, string email, string firstName, string lastName)
    {
        UserId = userId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }
}
