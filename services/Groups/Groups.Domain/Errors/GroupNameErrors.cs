using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Domain.Errors;

public static class GroupNameErrors
{
    public static readonly Error Empty = new(
        "GroupName.Empty",
        "El nombre del grupo no puede estar vacío.");

    public static readonly Error TooShort = new(
        "GroupName.TooShort",
        "El nombre del grupo debe tener al menos 3 caracteres.");

    public static readonly Error TooLong = new(
        "GroupName.TooLong",
        "El nombre del grupo no puede superar los 100 caracteres.");
}
