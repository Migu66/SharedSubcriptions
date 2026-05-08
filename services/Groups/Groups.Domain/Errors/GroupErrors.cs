using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Domain.Errors;

public static class GroupErrors
{
    public static readonly Error NotFound = new(
        "Group.NotFound",
        "El grupo no existe.");

    public static readonly Error NotAdmin = new(
        "Group.NotAdmin",
        "Solo el administrador del grupo puede realizar esta acción.");

    public static readonly Error MemberAlreadyExists = new(
        "Group.MemberAlreadyExists",
        "El usuario ya es miembro de este grupo.");

    public static readonly Error MemberNotFound = new(
        "Group.MemberNotFound",
        "El miembro no existe en este grupo.");

    public static readonly Error AdminCannotBeRemoved = new(
        "Group.AdminCannotBeRemoved",
        "El administrador del grupo no puede ser eliminado como miembro.");

    public static readonly Error NameRequired = new(
        "Group.NameRequired",
        "El nombre del grupo es obligatorio.");

    public static readonly Error NameTooShort = new(
        "Group.NameTooShort",
        "El nombre del grupo debe tener al menos 3 caracteres.");

    public static readonly Error NameTooLong = new(
        "Group.NameTooLong",
        "El nombre del grupo no puede superar los 100 caracteres.");
}
