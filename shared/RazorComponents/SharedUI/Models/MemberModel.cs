namespace SharedUI.Models;

public sealed record MemberModel(
    Guid Id,
    string Email,
    string Role,
    DateTime JoinedAt,
    PaymentStatus PaymentStatus);
