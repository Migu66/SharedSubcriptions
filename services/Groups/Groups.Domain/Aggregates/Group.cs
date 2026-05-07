using Groups.Domain.Enums;
using Groups.Domain.Errors;
using Groups.Domain.Events;
using Groups.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Domain.Aggregates;

public sealed class Group : AggregateRoot<GroupId>
{
    private readonly List<Member> _members = [];

    public GroupName Name { get; private set; } = null!;
    public UserId AdminId { get; private init; } = null!;
    public DateTime CreatedAt { get; private init; }

    public IReadOnlyCollection<Member> Members => _members.AsReadOnly();

    private Group() { }

    public static Result<Group> Create(
        GroupName name,
        UserId adminId,
        string adminEmail,
        DateTime createdAt)
    {
        var group = new Group
        {
            Id = GroupId.New(),
            Name = name,
            AdminId = adminId,
            CreatedAt = createdAt
        };

        var adminMemberResult = Member.Create(adminId, adminEmail, GroupRole.Admin, createdAt);
        if (adminMemberResult.IsFailure)
            return Result.Failure<Group>(adminMemberResult.Error);

        group._members.Add(adminMemberResult.Value);

        group.RaiseDomainEvent(new GroupCreatedEvent(
            EventId: Guid.NewGuid(),
            OccurredOn: createdAt,
            GroupId: group.Id,
            AdminId: adminId));

        return Result.Success(group);
    }

    public Result AddMember(UserId userId, string email, DateTime joinedAt)
    {
        bool alreadyExists = _members.Exists(m => m.Id == userId);
        if (alreadyExists)
            return Result.Failure(GroupErrors.MemberAlreadyExists);

        var memberResult = Member.Create(userId, email, GroupRole.Member, joinedAt);
        if (memberResult.IsFailure)
            return Result.Failure(memberResult.Error);

        _members.Add(memberResult.Value);

        RaiseDomainEvent(new MemberAddedEvent(
            EventId: Guid.NewGuid(),
            OccurredOn: joinedAt,
            GroupId: Id,
            UserId: userId,
            Email: email));

        return Result.Success();
    }

    public Result RemoveMember(UserId userId)
    {
        if (userId == AdminId)
            return Result.Failure(GroupErrors.AdminCannotBeRemoved);

        var member = _members.Find(m => m.Id == userId);
        if (member is null)
            return Result.Failure(GroupErrors.MemberNotFound);

        _members.Remove(member);

        RaiseDomainEvent(new MemberRemovedEvent(
            EventId: Guid.NewGuid(),
            OccurredOn: DateTime.UtcNow,
            GroupId: Id,
            UserId: userId));

        return Result.Success();
    }
}
