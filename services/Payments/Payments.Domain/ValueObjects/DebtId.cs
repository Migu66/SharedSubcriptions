using Payments.Domain.Errors;
using SharedSubscriptions.SharedKernel.Domain;

namespace Payments.Domain.ValueObjects;

public record DebtId(Guid Value)
{
    public static DebtId New() => new(Guid.NewGuid());

    public static Result<DebtId> From(Guid value)
    {
        if (value == Guid.Empty)
            return Result.Failure<DebtId>(DebtIdErrors.Empty);

        return Result.Success<DebtId>(new DebtId(value));
    }
}
