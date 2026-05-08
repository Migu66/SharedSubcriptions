namespace SharedSubscriptions.SharedKernel.Domain;

/// <summary>
/// Contrato no genérico para que la infraestructura pueda recolectar
/// y despachar domain events sin necesidad de conocer el tipo de ID.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
