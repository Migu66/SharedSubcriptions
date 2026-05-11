using Notifications.Domain.Errors;
using SharedSubscriptions.SharedKernel.Domain;

namespace Notifications.Domain.ValueObjects;

public record NotificationId(Guid Value)
{
    public static NotificationId New() => new(Guid.NewGuid());

    public static Result<NotificationId> From(Guid value)
    {
        if (value == Guid.Empty)
            return Result.Failure<NotificationId>(NotificationIdErrors.Empty);

        return Result.Success<NotificationId>(new NotificationId(value));
    }
}
