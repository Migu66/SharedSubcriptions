using Identity.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Identity.Domain.Events;

public sealed record UserRegisteredEvent(
    Guid EventId,
    DateTime OccurredOn,
    UserId UserId,
    string Email,
    string FirstName,
    string LastName) : IDomainEvent;
