using Payments.Domain.Errors;
using SharedSubscriptions.SharedKernel.Domain;

namespace Payments.Domain.ValueObjects;

/// <summary>
/// Referencia al identificador de una suscripción del Subscriptions Service.
/// </summary>
public record SubscriptionId(Guid Value)
{
    public static SubscriptionId New() => new(Guid.NewGuid());

    public static Result<SubscriptionId> From(Guid value)
    {
        if (value == Guid.Empty)
            return Result.Failure<SubscriptionId>(SubscriptionIdErrors.Empty);

        return Result.Success<SubscriptionId>(new SubscriptionId(value));
    }
}
