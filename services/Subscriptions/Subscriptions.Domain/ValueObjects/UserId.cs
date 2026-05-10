using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.Errors;

namespace Subscriptions.Domain.ValueObjects;

/// <summary>
/// Referencia al identificador de un usuario del Identity Service.
/// </summary>
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
