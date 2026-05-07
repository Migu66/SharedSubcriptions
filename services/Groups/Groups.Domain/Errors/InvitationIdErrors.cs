using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Domain.Errors;

public static class InvitationIdErrors
{
    public static readonly Error Empty = new(
        "InvitationId.Empty",
        "El identificador de la invitación no puede estar vacío.");
}
