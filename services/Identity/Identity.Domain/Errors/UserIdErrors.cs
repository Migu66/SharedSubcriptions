using SharedSubscriptions.SharedKernel.Domain;

namespace Identity.Domain.Errors;

public static class UserIdErrors
{
    public static readonly Error Empty = new(
        "UserId.Empty",
        "El identificador del usuario no puede estar vacío.");
}
