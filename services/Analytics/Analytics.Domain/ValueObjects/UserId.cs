using Analytics.Domain.Errors;
using SharedSubscriptions.SharedKernel.Domain;

namespace Analytics.Domain.ValueObjects;

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
