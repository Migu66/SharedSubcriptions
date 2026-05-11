namespace Analytics.Application.DTOs;

public sealed record GroupSavingsDto(
    Guid GroupId,
    int Year,
    decimal TotalSpent,
    decimal EstimatedSavings);
