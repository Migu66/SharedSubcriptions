using Groups.Domain.Enums;
using Groups.Domain.Errors;
using Groups.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Domain.Aggregates;

public sealed class Invitation : AggregateRoot<InvitationId>
{
    public GroupId GroupId { get; private init; } = null!;
    public string InviteeEmail { get; private init; } = string.Empty;
    public InvitationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime ExpiresAt { get; private init; }

    private Invitation() { }

    public static Result<Invitation> Create(
        GroupId groupId,
        string inviteeEmail,
        DateTime createdAt,
        DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(inviteeEmail))
            return Result.Failure<Invitation>(InvitationErrors.EmailEmpty);

        if (expiresAt <= createdAt)
            return Result.Failure<Invitation>(InvitationErrors.InvalidExpiryDate);

        var invitation = new Invitation
        {
            Id = InvitationId.New(),
            GroupId = groupId,
            InviteeEmail = inviteeEmail,
            Status = InvitationStatus.Pending,
            CreatedAt = createdAt,
            ExpiresAt = expiresAt
        };

        return Result.Success(invitation);
    }

    public Result Accept(DateTime now)
    {
        if (Status == InvitationStatus.Accepted)
            return Result.Failure(InvitationErrors.AlreadyAccepted);

        if (Status == InvitationStatus.Cancelled)
            return Result.Failure(InvitationErrors.AlreadyCancelled);

        if (now > ExpiresAt)
            return Result.Failure(InvitationErrors.Expired);

        Status = InvitationStatus.Accepted;

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == InvitationStatus.Accepted)
            return Result.Failure(InvitationErrors.AlreadyAccepted);

        if (Status == InvitationStatus.Cancelled)
            return Result.Failure(InvitationErrors.AlreadyCancelled);

        Status = InvitationStatus.Cancelled;

        return Result.Success();
    }
}
