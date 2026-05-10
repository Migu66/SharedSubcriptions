using SharedSubscriptions.SharedKernel.Domain;

namespace Payments.Domain.Errors;

public static class UserIdErrors
{
    public static readonly Error Empty = new(
        "UserId.Empty",
        "El identificador de usuario no puede estar vacío.");
}
