namespace SharedSubscriptions.SharedKernel.Domain;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
