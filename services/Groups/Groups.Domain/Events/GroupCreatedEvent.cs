using Groups.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Domain.Events;

public sealed record GroupCreatedEvent(
    Guid EventId,
    DateTime OccurredOn,
    GroupId GroupId,
    UserId AdminId) : IDomainEvent;
