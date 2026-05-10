using SharedSubscriptions.SharedKernel.Domain;

namespace Subscriptions.Domain.Errors;

public static class SubscriptionErrors
{
    public static readonly Error NotFound = new(
        "Subscription.NotFound",
        "La suscripción no existe.");

    public static readonly Error ServiceNameEmpty = new(
        "Subscription.ServiceNameEmpty",
        "El nombre del servicio no puede estar vacío.");

    public static readonly Error ServiceNameTooLong = new(
        "Subscription.ServiceNameTooLong",
        "El nombre del servicio no puede superar los 100 caracteres.");

    public static readonly Error AlreadyInactive = new(
        "Subscription.AlreadyInactive",
        "La suscripción ya está desactivada.");
}
