using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Domain.ValueObjects;

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

internal static class GroupIdErrors
{
    internal static readonly Error Empty = new(
        "GroupId.Empty",
        "El identificador del grupo no puede estar vacío.");
}
