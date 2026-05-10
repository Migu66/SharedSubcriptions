using Payments.Domain.Errors;
using SharedSubscriptions.SharedKernel.Domain;

namespace Payments.Domain.ValueObjects;

public record PaymentRecordId(Guid Value)
{
    public static PaymentRecordId New() => new(Guid.NewGuid());

    public static Result<PaymentRecordId> From(Guid value)
    {
        if (value == Guid.Empty)
            return Result.Failure<PaymentRecordId>(PaymentRecordIdErrors.Empty);

        return Result.Success<PaymentRecordId>(new PaymentRecordId(value));
    }
}
