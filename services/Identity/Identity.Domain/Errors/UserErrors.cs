using SharedSubscriptions.SharedKernel.Domain;

namespace Identity.Domain.Errors;

public static class UserErrors
{
    public static readonly Error EmailAlreadyExists = new(
        "User.EmailAlreadyExists",
        "Ya existe un usuario registrado con ese email.");

    public static readonly Error InvalidEmail = new(
        "User.InvalidEmail",
        "El formato del email no es válido.");

    public static readonly Error NotFound = new(
        "User.NotFound",
        "El usuario no existe.");
}
