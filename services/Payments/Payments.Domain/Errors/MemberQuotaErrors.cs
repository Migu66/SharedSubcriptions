using SharedSubscriptions.SharedKernel.Domain;

namespace Payments.Domain.Errors;

public static class MemberQuotaErrors
{
    public static readonly Error InvalidMemberCount = new(
        "MemberQuota.InvalidMemberCount",
        "El número de miembros debe ser mayor que cero.");

    public static readonly Error InvalidDays = new(
        "MemberQuota.InvalidDays",
        "Los días restantes no pueden ser negativos ni superar el total de días del ciclo.");
}
