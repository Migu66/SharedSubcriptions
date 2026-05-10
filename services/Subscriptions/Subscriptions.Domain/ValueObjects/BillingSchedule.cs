using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.Errors;
using Subscriptions.Domain.Enums;

namespace Subscriptions.Domain.ValueObjects;

public record BillingSchedule
{
    public BillingCycle Cycle { get; }
    public DateTime NextBillingDate { get; }

    private BillingSchedule(BillingCycle cycle, DateTime nextBillingDate)
    {
        Cycle = cycle;
        NextBillingDate = nextBillingDate;
    }

    public static Result<BillingSchedule> Create(BillingCycle cycle, DateTime nextBillingDate)
    {
        if (nextBillingDate == default)
            return Result.Failure<BillingSchedule>(BillingScheduleErrors.InvalidDate);

        return Result.Success<BillingSchedule>(new BillingSchedule(cycle, nextBillingDate));
    }

    public BillingSchedule CalculateNextBillingDate()
    {
        var nextDate = Cycle == BillingCycle.Monthly
            ? NextBillingDate.AddMonths(1)
            : NextBillingDate.AddYears(1);

        return new BillingSchedule(Cycle, nextDate);
    }
}
