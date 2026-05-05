namespace SharedSubscriptions.SharedKernel.Domain;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
