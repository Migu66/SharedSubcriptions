using Groups.Domain.Enums;
using Groups.Domain.Errors;
using Groups.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Domain.Aggregates;

public sealed class Member : Entity<UserId>
{
    public string Email { get; private init; } = string.Empty;
    public GroupRole Role { get; private init; }
    public DateTime JoinedAt { get; private init; }

    private Member() { }

    public static Result<Member> Create(
        UserId userId,
        string email,
        GroupRole role,
        DateTime joinedAt)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<Member>(MemberErrors.EmailEmpty);

        var member = new Member
        {
            Id = userId,
            Email = email,
            Role = role,
            JoinedAt = joinedAt
        };

        return Result.Success(member);
    }
}
