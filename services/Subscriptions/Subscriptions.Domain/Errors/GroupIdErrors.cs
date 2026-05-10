using SharedSubscriptions.SharedKernel.Domain;

namespace Subscriptions.Domain.Errors;

public static class GroupIdErrors
{
    public static readonly Error Empty = new(
        "GroupId.Empty",
        "El identificador del grupo no puede estar vacío.");
}
