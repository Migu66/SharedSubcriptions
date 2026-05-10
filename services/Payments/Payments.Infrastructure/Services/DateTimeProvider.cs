using SharedSubscriptions.SharedKernel.Domain;

namespace Payments.Infrastructure.Services;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
