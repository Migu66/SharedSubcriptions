using SharedSubscriptions.SharedKernel.Domain;

namespace Payments.Domain.Errors;

public static class DebtIdErrors
{
    public static readonly Error Empty = new(
        "DebtId.Empty",
        "El identificador de la deuda no puede estar vacío.");
}
