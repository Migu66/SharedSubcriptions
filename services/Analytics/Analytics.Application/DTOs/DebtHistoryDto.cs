namespace Analytics.Application.DTOs;

public sealed record DebtHistoryDto(
    Guid UserId,
    decimal TotalDebt,
    decimal TotalSettled,
    int PendingCount);
