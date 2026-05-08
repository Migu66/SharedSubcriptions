using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Infrastructure.Services;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
