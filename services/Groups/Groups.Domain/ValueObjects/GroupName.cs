using Groups.Domain.Errors;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Domain.ValueObjects;

public record GroupName
{
    public string Value { get; }

    private GroupName(string value)
    {
        Value = value;
    }

    public static Result<GroupName> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<GroupName>(GroupNameErrors.Empty);

        if (value.Length < 3)
            return Result.Failure<GroupName>(GroupNameErrors.TooShort);

        if (value.Length > 100)
            return Result.Failure<GroupName>(GroupNameErrors.TooLong);

        return Result.Success<GroupName>(new GroupName(value));
    }
}
