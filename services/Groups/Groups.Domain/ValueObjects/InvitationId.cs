using Groups.Domain.Errors;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Domain.ValueObjects;

public record InvitationId(Guid Value)
{
    public static InvitationId New() => new(Guid.NewGuid());

    public static Result<InvitationId> From(Guid value)
    {
        if (value == Guid.Empty)
            return Result.Failure<InvitationId>(InvitationIdErrors.Empty);

        return Result.Success<InvitationId>(new InvitationId(value));
    }
}
