using SharedSubscriptions.SharedKernel.Domain;

namespace Identity.Infrastructure.Services;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
