using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.Errors;

namespace Subscriptions.Domain.ValueObjects;

/// <summary>
/// Referencia al identificador de un grupo del Groups Service.
/// </summary>
public record GroupId(Guid Value)
{
    public static GroupId New() => new(Guid.NewGuid());

    public static Result<GroupId> From(Guid value)
    {
        if (value == Guid.Empty)
            return Result.Failure<GroupId>(GroupIdErrors.Empty);

        return Result.Success<GroupId>(new GroupId(value));
    }
}
