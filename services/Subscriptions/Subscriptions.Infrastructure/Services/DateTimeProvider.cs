using SharedSubscriptions.SharedKernel.Domain;

namespace Subscriptions.Infrastructure.Services;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
