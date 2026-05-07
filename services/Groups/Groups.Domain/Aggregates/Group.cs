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
        DateTime createdAt)
    {
        var group = new Group
        {
            Id = GroupId.New(),
            Name = name,
            AdminId = adminId,
            CreatedAt = createdAt
        };

        var adminMemberResult = Member.Create(adminId, string.Empty, GroupRole.Admin, createdAt);
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
}
