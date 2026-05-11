using SharedSubscriptions.SharedKernel.Domain;

namespace Notifications.Domain.Errors;

public static class NotificationIdErrors
{
    public static readonly Error Empty = new(
        "NotificationId.Empty",
        "El identificador de la notificación no puede estar vacío.");
}
