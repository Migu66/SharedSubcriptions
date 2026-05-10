using Payments.Domain.Errors;
using SharedSubscriptions.SharedKernel.Domain;

namespace Payments.Domain.ValueObjects;

public record UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());

    public static Result<UserId> From(Guid value)
    {
        if (value == Guid.Empty)
            return Result.Failure<UserId>(UserIdErrors.Empty);

        return Result.Success<UserId>(new UserId(value));
    }
}
