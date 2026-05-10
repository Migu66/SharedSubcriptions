using Payments.Domain.Errors;
using SharedSubscriptions.SharedKernel.Domain;

namespace Payments.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Result<Money> Create(decimal amount, string? currency)
    {
        if (amount < 0)
            return Result.Failure<Money>(MoneyErrors.NegativeAmount);

        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure<Money>(MoneyErrors.EmptyCurrency);

        if (currency.Trim().Length != 3)
            return Result.Failure<Money>(MoneyErrors.InvalidCurrencyFormat);

        return Result.Success<Money>(new Money(amount, currency.Trim().ToUpperInvariant()));
    }
}
