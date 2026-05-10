using SharedSubscriptions.SharedKernel.Domain;

namespace Subscriptions.Domain.Errors;

public static class SubscriptionIdErrors
{
    public static readonly Error Empty = new(
        "SubscriptionId.Empty",
        "El identificador de la suscripción no puede estar vacío.");
}
