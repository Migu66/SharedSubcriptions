namespace SharedUI.Models;

public sealed record GroupSavingsModel(
    Guid GroupId,
    int Year,
    decimal TotalSpent,
    decimal EstimatedSavings,
    string Currency);
