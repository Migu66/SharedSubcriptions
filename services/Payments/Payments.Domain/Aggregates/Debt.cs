using SharedSubscriptions.SharedKernel.Domain;
using Payments.Domain.Enums;
using Payments.Domain.Errors;
using Payments.Domain.Events;
using Payments.Domain.ValueObjects;

namespace Payments.Domain.Aggregates;

public sealed class Debt : AggregateRoot<DebtId>
{
    public PaymentRecordId PaymentRecordId { get; private init; } = null!;
    public UserId DebtorId { get; private init; } = null!;
    public UserId CreditorId { get; private init; } = null!;
    public Money Amount { get; private init; } = null!;
    public DebtStatus Status { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? SettledAt { get; private set; }

    private Debt() { }

    public static Result<Debt> Create(
        PaymentRecordId paymentRecordId,
        UserId debtorId,
        UserId creditorId,
        Money amount,
        DateTime createdAt)
    {
        var debt = new Debt
        {
            Id = DebtId.New(),
            PaymentRecordId = paymentRecordId,
            DebtorId = debtorId,
            CreditorId = creditorId,
            Amount = amount,
            Status = DebtStatus.Pending,
            CreatedAt = createdAt,
            SettledAt = null
        };

        return Result.Success(debt);
    }

    public Result Settle(DateTime settledAt)
    {
        if (Status == DebtStatus.Settled)
            return Result.Failure(DebtErrors.AlreadySettled);

        if (Status == DebtStatus.Cancelled)
            return Result.Failure(DebtErrors.AlreadyCancelled);

        Status = DebtStatus.Settled;
        SettledAt = settledAt;

        RaiseDomainEvent(new DebtSettledEvent(
            EventId: Guid.NewGuid(),
            OccurredOn: settledAt,
            DebtId: Id,
            DebtorId: DebtorId,
            CreditorId: CreditorId,
            Amount: Amount));

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == DebtStatus.Settled)
            return Result.Failure(DebtErrors.AlreadySettled);

        if (Status == DebtStatus.Cancelled)
            return Result.Failure(DebtErrors.AlreadyCancelled);

        Status = DebtStatus.Cancelled;

        return Result.Success();
    }
}
