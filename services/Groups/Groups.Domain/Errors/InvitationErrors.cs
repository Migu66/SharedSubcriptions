using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Domain.Errors;

public static class InvitationErrors
{
    public static readonly Error EmailEmpty = new(
        "Invitation.EmailEmpty",
        "El email del invitado no puede estar vacío.");

    public static readonly Error AlreadyAccepted = new(
        "Invitation.AlreadyAccepted",
        "La invitación ya ha sido aceptada.");

    public static readonly Error AlreadyCancelled = new(
        "Invitation.AlreadyCancelled",
        "La invitación ya ha sido cancelada.");

    public static readonly Error Expired = new(
        "Invitation.Expired",
        "La invitación ha expirado y no puede ser aceptada.");

    public static readonly Error InvalidExpiryDate = new(
        "Invitation.InvalidExpiryDate",
        "La fecha de expiración debe ser posterior a la fecha de creación.");
}
