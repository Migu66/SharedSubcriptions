using SharedSubscriptions.SharedKernel.Domain;

namespace Subscriptions.Domain.Errors;

public static class BillingScheduleErrors
{
    public static readonly Error InvalidDate = new(
        "BillingSchedule.InvalidDate",
        "La fecha de próximo cobro no es válida.");
}
