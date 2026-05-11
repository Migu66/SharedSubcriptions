namespace Analytics.Application.DTOs;

public sealed record ServiceSpendingDto(
    Guid GroupId,
    string ServiceName,
    decimal TotalSpent,
    int PaymentCount);
