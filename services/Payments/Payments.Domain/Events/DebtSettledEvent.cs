using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.ValueObjects;

namespace Payments.Domain.Events;

public sealed record DebtSettledEvent(
    Guid EventId,
    DateTime OccurredOn,
    DebtId DebtId,
    UserId DebtorId,
    UserId CreditorId,
    Money Amount) : IDomainEvent;
