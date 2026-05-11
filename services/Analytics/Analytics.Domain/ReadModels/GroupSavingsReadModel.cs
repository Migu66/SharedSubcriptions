using Analytics.Domain.ValueObjects;

namespace Analytics.Domain.ReadModels;

/// <summary>
/// Proyección de solo lectura que acumula el gasto total y el ahorro estimado
/// de un grupo en un año concreto.
/// Actualizado al recibir PaymentConfirmedIntegrationEvent.
/// </summary>
public sealed class GroupSavingsReadModel
{
    public Guid Id { get; init; }
    public GroupId GroupId { get; init; } = default!;
    public int Year { get; init; }
    public decimal TotalSpent { get; private set; }
    public decimal EstimatedSavings { get; private set; }

    // Constructor vacío para EF Core
    private GroupSavingsReadModel() { }

    public static GroupSavingsReadModel Create(GroupId groupId, int year)
        => new()
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            Year = year,
            TotalSpent = 0m,
            EstimatedSavings = 0m
        };

    public void AddPayment(decimal amount, decimal estimatedIndividualCost)
    {
        TotalSpent += amount;
        EstimatedSavings += estimatedIndividualCost - amount;
    }
}
