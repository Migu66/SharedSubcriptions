using Payments.Domain.Errors;
using SharedSubscriptions.SharedKernel.Domain;

namespace Payments.Domain.ValueObjects;

public record MemberQuota
{
    public UserId MemberId { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public bool IsProrrated { get; private set; }

    private MemberQuota() { }

    private MemberQuota(UserId memberId, decimal amount, string currency, bool isProrrated)
    {
        MemberId = memberId;
        Amount = amount;
        Currency = currency;
        IsProrrated = isProrrated;
    }

    public static Result<MemberQuota> Create(UserId memberId, decimal amount, string currency, bool isProrrated)
    {
        var moneyResult = Money.Create(amount, currency);
        if (moneyResult.IsFailure)
            return Result.Failure<MemberQuota>(moneyResult.Error);

        return Result.Success<MemberQuota>(new MemberQuota(memberId, amount, currency.Trim().ToUpperInvariant(), isProrrated));
    }

    public static Result<MemberQuota> Calculate(UserId memberId, Money totalCost, int memberCount)
    {
        if (memberCount <= 0)
            return Result.Failure<MemberQuota>(MemberQuotaErrors.InvalidMemberCount);

        var amount = Math.Round(totalCost.Amount / memberCount, 2);
        return Result.Success<MemberQuota>(new MemberQuota(memberId, amount, totalCost.Currency, isProrrated: false));
    }

    public static Result<MemberQuota> CalculateProrrated(UserId memberId, Money totalCost, int memberCount, int remainingDays, int totalDays)
    {
        if (memberCount <= 0)
            return Result.Failure<MemberQuota>(MemberQuotaErrors.InvalidMemberCount);

        if (remainingDays < 0 || remainingDays > totalDays || totalDays <= 0)
            return Result.Failure<MemberQuota>(MemberQuotaErrors.InvalidDays);

        var baseAmount = totalCost.Amount / memberCount;
        var proratedAmount = Math.Round(baseAmount * remainingDays / totalDays, 2);
        return Result.Success<MemberQuota>(new MemberQuota(memberId, proratedAmount, totalCost.Currency, isProrrated: true));
    }
}
