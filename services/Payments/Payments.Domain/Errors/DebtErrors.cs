using SharedSubscriptions.SharedKernel.Domain;

namespace Payments.Domain.Errors;

public static class DebtErrors
{
    public static readonly Error NotFound = new(
        "Debt.NotFound",
        "La deuda no existe.");

    public static readonly Error AlreadySettled = new(
        "Debt.AlreadySettled",
        "La deuda ya ha sido saldada.");

    public static readonly Error AlreadyCancelled = new(
        "Debt.AlreadyCancelled",
        "La deuda ya ha sido cancelada.");

    public static readonly Error NotDebtor = new(
        "Debt.NotDebtor",
        "Solo el deudor puede saldar su propia deuda.");

    public static readonly Error NotCreditor = new(
        "Debt.NotCreditor",
        "Solo el acreedor puede marcar la deuda como saldada manualmente.");
}
