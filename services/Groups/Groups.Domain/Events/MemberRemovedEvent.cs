using Groups.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Domain.Events;

public sealed record MemberRemovedEvent(
    Guid EventId,
    DateTime OccurredOn,
    GroupId GroupId,
    UserId UserId) : IDomainEvent;
