using Payments.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Payments.Application.Abstractions;

/// <summary>
/// Contrato para el servicio de pagos online con Stripe.
/// La implementación vive en Infrastructure para no contaminar el dominio.
/// </summary>
public interface IStripePaymentService
{
    /// <summary>
    /// Crea un PaymentIntent en Stripe y devuelve el client_secret
    /// que el cliente necesita para completar el pago desde el frontend.
    /// </summary>
    Task<Result<string>> CreatePaymentIntentAsync(
        Money amount,
        UserId debtorId,
        DebtId debtId,
        CancellationToken cancellationToken = default);
}
