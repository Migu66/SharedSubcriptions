namespace SharedUI.Models;

public sealed record GroupModel(
    Guid Id,
    string Name,
    Guid AdminId,
    DateTime CreatedAt,
    IReadOnlyList<MemberModel> Members);
