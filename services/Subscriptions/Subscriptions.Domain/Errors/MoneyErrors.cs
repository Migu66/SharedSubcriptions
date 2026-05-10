using SharedSubscriptions.SharedKernel.Domain;

namespace Subscriptions.Domain.Errors;

public static class MoneyErrors
{
    public static readonly Error NegativeAmount = new(
        "Money.NegativeAmount",
        "El importe no puede ser negativo.");

    public static readonly Error EmptyCurrency = new(
        "Money.EmptyCurrency",
        "La moneda no puede estar vacía.");

    public static readonly Error InvalidCurrencyFormat = new(
        "Money.InvalidCurrencyFormat",
        "La moneda debe ser un código ISO de tres letras (por ejemplo, EUR, USD).");
}
