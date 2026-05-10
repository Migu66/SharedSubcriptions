using SharedSubscriptions.SharedKernel.Domain;

namespace Payments.Domain.Errors;

public static class PaymentRecordIdErrors
{
    public static readonly Error Empty = new(
        "PaymentRecordId.Empty",
        "El identificador del registro de pago no puede estar vacío.");
}
