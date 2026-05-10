using SharedSubscriptions.SharedKernel.Domain;

namespace Payments.Domain.Errors;

public static class PaymentRecordErrors
{
    public static readonly Error NotFound = new(
        "PaymentRecord.NotFound",
        "El registro de pago no existe.");

    public static readonly Error EmptyQuotas = new(
        "PaymentRecord.EmptyQuotas",
        "El registro de pago debe incluir al menos una cuota de miembro.");
}
